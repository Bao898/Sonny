// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Autodesk.Revit.DB.Structure;
using MoreLinq.Extensions;
using Binding = Autodesk.Revit.DB.Binding;

namespace SonnyBIM;

/// <summary>
/// Chỉnh đầu dầm theo tường hoặc cột.
/// CHANGED: phân loại theo từng đầu dầm (start/end), không khóa cả selection:
/// 1) Gặp tường → tường–dầm
/// 2a) CheckBox ON + 3 dầm–1 cột → cặp đồng line thụt G + cắt Width Ledge
/// 2) Gần tâm cột đúng 2 dầm → dầm–dầm–cột (G)
/// 3) Gác mép cột → cột–dầm
/// 4) Không gặp → bỏ qua đầu đó
/// Không dùng Start/End Extension.
/// </summary>
public static class AdjustStrBeamServices
{
    // Dầm coi là vuông góc khi góc dầm–tường lệch khỏi 90° dưới 0.1 độ.
    private const double AngleTolDeg = 0.1;

    // Case 1: đầu dầm coi là "chạm mặt tường" nếu cách mặt < khoảng này (mm)
    private const double TouchTolMm = 10.0;

    // Case 2: sai số khi kiểm tra dist_ngoài + dist_trong ≈ Width (mm)
    private const double InsideWallTolMm = 1.0;

    // Case 3: chiều sâu profile dư thêm quá endpoint (mm).
    private const double OpeningProfileExtension = 400.0;

    // CHANGED: Case 3 — đầu Line ngoài tường nhưng cách mặt ngoài <= 10 mm vẫn coi là giao.
    private const double SkewOutsideTouchTolMm = 10.0;

    // CHANGED: Case 3 — cạnh bên hình bình hành cách biên dạng dầm mỗi bên 100 mm.
    private const double SkewSideClearanceMm = 200.0;

    // CHANGED: Opening cắt Width Ledge — dư thêm mỗi cạnh (mm).
    private const double LedgeOpeningExtraMm = 10.0;

    // CHANGED: cạnh xa cột — sàn / trần cách mặt cột (mm); nhận sFar ∈ [min, max].
    private const double LedgeOpeningMinFromColumnFaceMm = 300.0;
    private const double LedgeOpeningMaxFromColumnFaceMm = 500.0;

    // CHANGED: 2 dầm đồng line — góc giữa 2 hướng < 3°.
    private const double CollinearBeamAngleTolDeg = 3.0;

    // CHANGED: Case 1 GetBeamPlanWidth — danh sách tên param bề rộng (thêm tên mới tại đây).
    private static readonly string[] BeamWidthParameterNames = { "Width", "b", "B" };

    // <summary>
    /// Kết quả tìm mặt tường đã chạm để neo gap.
    /// </summary>
    private struct WallFaceHit
    {
        public XYZ FacePoint;
    }

    /// <summary>
    /// CHANGED: điểm vào chung — chạy tuần tự các pha, đánh dấu đầu đã xử lý.
    /// </summary>
    /// <param name="useExteriorFace">true = mặt ngoài tường; false = mặt trong (chỉ luồng tường)</param>
    /// <param name="beamBeamGapColumnMm">CHANGED: tổng gap G (mm) giữa 2 đầu dầm trên cột.</param>
    /// <param name="beamBeamPerpendicularGapMm">CHANGED: gap dầm thứ 3 so với mặt stem (mm).</param>
    /// <param name="cutWidthLedgeAtPillar">
    /// CHANGED: CheckBox — OFF = không chạy bất kỳ case nào; ON = chạy đủ pha (gồm cắt Width Ledge).
    /// </param>
    public static void Execute(
        Document doc,
        IList<FamilyInstance> beams,
        IList<Wall> walls,
        IList<FamilyInstance> columns,
        double wallGapMm,
        double columnGapMm,
        double beamBeamGapColumnMm,
        double beamBeamPerpendicularGapMm,
        bool cutWidthLedgeAtPillar,
        bool useExteriorFace)
    {
        if (doc == null || beams == null || !beams.Any())
            return;

        // CHANGED: CheckBox OFF → không tường / cột / dầm–dầm / ledge / dầm 3.
        if (!cutWidthLedgeAtPillar)
            return;

        // CHANGED: đầu (beamId, endIndex) đã xử lý — tường / dầm–dầm không bị cột–mép đè.
        var handledEnds = new HashSet<(int BeamId, int EndIndex)>();

        // ----- 1) Tường–dầm (chỉ đầu gặp tường; không chặn pha cột) -----
        if (walls != null && walls.Any() && wallGapMm >= 0)
            ExecuteBeamWall(doc, beams, walls, wallGapMm, useExteriorFace, handledEnds);

        if (columns == null || !columns.Any())
            return;

        // ----- 2a) 3 dầm–1 cột — ledge hosts + gap dầm thứ 3 (stem) -----
        if (beamBeamGapColumnMm >= 0 || beamBeamPerpendicularGapMm >= 0)
        {
            List<ThreeBeamColumnGroup> threeGroups =
                CollectThreeBeamColumnGroups(beams, columns, handledEnds);
            foreach (ThreeBeamColumnGroup group in threeGroups)
                ExecuteThreeBeamColumnLedgeCheckbox(
                    doc, group, beamBeamGapColumnMm, beamBeamPerpendicularGapMm, handledEnds);
        }

        // ----- 2) Dầm–dầm–cột: mỗi cột đúng 2 dầm có đầu gần tâm -----
        if (beamBeamGapColumnMm >= 0)
        {
            List<BeamBeamColumnGroup> groups =
                CollectBeamBeamColumnGroups(beams, columns, handledEnds);
            foreach (BeamBeamColumnGroup group in groups)
                ExecuteBeamBeamColumn(
                    doc, group.Beams, group.Column, beamBeamGapColumnMm, handledEnds);
        }

        // ----- 3) Cột–dầm gác mép: chỉ đầu chưa handled -----
        if (columnGapMm >= 0)
            ExecuteBeamColumn(doc, beams, columns, columnGapMm, handledEnds);
    }

    /// <summary>CHANGED: luồng tường; đánh dấu đầu gặp tường vào handledEnds.</summary>
    private static void ExecuteBeamWall(
        Document doc,
        IList<FamilyInstance> beams,
        IList<Wall> walls,
        double gapMm,
        bool useExteriorFace,
        HashSet<(int BeamId, int EndIndex)> handledEnds)
    {
        double gapFt = gapMm / 304.8;
        double touchTolFt = TouchTolMm / 304.8;
        double insideTolFt = InsideWallTolMm / 304.8;

        foreach (FamilyInstance beam in beams)
        {
            if (beam == null || !beam.IsValidObject)
                continue;

            PrepareBeamEnds(beam);

            if (beam.Location is not LocationCurve location)
                continue;
            if (location.Curve is not Line line)
                continue;

            XYZ p0 = line.GetEndPoint(0);
            XYZ p1 = line.GetEndPoint(1);
            XYZ dir = (p1 - p0).Normalize();

            // CHANGED: đánh dấu đầu gặp tường TRƯỚC khi chỉnh (ưu tiên case tường).
            MarkEndsMeetingWalls(
                beam, p0, p1, dir, walls, touchTolFt, insideTolFt, useExteriorFace, handledEnds);

            bool createdSkewOpening = TryCutSkewBeamByOpening(
                doc, beam, line, walls, gapFt, insideTolFt);
            if (createdSkewOpening)
                continue;

            AdjustEndIfTouchWall(ref p0, -dir, walls, gapFt, touchTolFt, insideTolFt, useExteriorFace);
            AdjustEndIfTouchWall(ref p1, dir, walls, gapFt, touchTolFt, insideTolFt, useExteriorFace);

            if (p0.DistanceTo(p1) < 0.01)
                continue;

            location.Curve = Line.CreateBound(p0, p1);
        }
    }

    /// <summary>CHANGED: đánh dấu endIndex gặp tường (perp hoặc trong/sát tường).</summary>
    private static void MarkEndsMeetingWalls(
        FamilyInstance beam,
        XYZ p0,
        XYZ p1,
        XYZ dir,
        IList<Wall> walls,
        double touchTolFt,
        double insideTolFt,
        bool useExteriorFace,
        HashSet<(int BeamId, int EndIndex)> handledEnds)
    {
        if (handledEnds == null || beam == null)
            return;

        int beamId = beam.Id.IntegerValue;
        double outsideTouchTolFt = SkewOutsideTouchTolMm / 304.8;

        bool meet0 = FindTargetWallFace(
                        p0, -dir, walls, touchTolFt, insideTolFt, useExteriorFace) != null;
        bool meet1 = FindTargetWallFace(
                        p1, dir, walls, touchTolFt, insideTolFt, useExteriorFace) != null;

        if (!meet0 || !meet1)
        {
            foreach (Wall wall in walls)
            {
                if (wall == null || !wall.IsValidObject)
                    continue;
                if (!meet0 && IsBeamEndMeetingWall(p0, wall, insideTolFt, outsideTouchTolFt))
                    meet0 = true;
                if (!meet1 && IsBeamEndMeetingWall(p1, wall, insideTolFt, outsideTouchTolFt))
                    meet1 = true;
            }
        }

        if (meet0) handledEnds.Add((beamId, 0));
        if (meet1) handledEnds.Add((beamId, 1));
    }

    /// <summary>CHANGED: DisallowJoin + Extension = 0 (dùng chung tường/cột).</summary>
    private static void PrepareBeamEnds(FamilyInstance beam)
    {
        StructuralFramingUtils.DisallowJoinAtEnd(beam, 0);
        StructuralFramingUtils.DisallowJoinAtEnd(beam, 1);
        beam.get_Parameter(BuiltInParameter.START_EXTENSION)?.Set(0.0);
        beam.get_Parameter(BuiltInParameter.END_EXTENSION)?.Set(0.0);
    }


    #region Beam-Beam on Column
    // =====================================================================
    // CHANGED: REGION Beam-Beam on Column (2 dầm gác giữa 1 cột)
    // UI G = tổng gap; mỗi dầm lùi G/2 từ tâm cột M.
    // Multi-select: gom nhóm theo từng cột (không còn bắt buộc cả selection = 2+1).
    // =====================================================================
    /// <summary>CHANGED: một nhóm 1 cột + đúng 2 dầm có đầu gần tâm cột.</summary>
    private struct BeamBeamColumnGroup
    {
        public FamilyInstance Column;
        public List<FamilyInstance> Beams;
    }

    /// <summary>
    /// CHANGED: quét mọi cột — cột nào có đúng 2 dầm (đầu gần tâm, chưa handled tường) → 1 nhóm.
    /// </summary>
    private static List<BeamBeamColumnGroup> CollectBeamBeamColumnGroups(
        IList<FamilyInstance> beams,
        IList<FamilyInstance> columns,
        HashSet<(int BeamId, int EndIndex)> handledEnds)
    {
        var groups = new List<BeamBeamColumnGroup>();
        if (beams == null || columns == null)
            return groups;

        foreach (FamilyInstance column in columns)
        {
            if (column == null || !column.IsValidObject)
                continue;
            if (!TryGetRectangularColumnPlan(
                    column, out XYZ center, out _, out _, out double halfX, out double halfY))
                continue;

            double nearCenterTolFt =
                Math.Max(halfX, halfY) * 0.5 + SkewOutsideTouchTolMm / 304.8;

            var pairBeams = new List<FamilyInstance>();
            foreach (FamilyInstance beam in beams)
            {
                if (beam == null || !beam.IsValidObject)
                    continue;
                if (!TryGetBeamEndNearPoint(
                        beam, center, out XYZ endNearColumn, out _, out _))
                    continue;
                if (SetZ(endNearColumn, 0).DistanceTo(SetZ(center, 0)) > nearCenterTolFt)
                    continue;

                int endIndex = GetBeamEndIndex(beam, endNearColumn);
                if (endIndex < 0)
                    continue;
                if (handledEnds != null &&
                    handledEnds.Contains((beam.Id.IntegerValue, endIndex)))
                    continue;

                if (!pairBeams.Contains(beam))
                    pairBeams.Add(beam);
            }

            if (pairBeams.Count == 2)
            {
                groups.Add(new BeamBeamColumnGroup
                {
                    Column = column,
                    Beams = pairBeams
                });
            }
        }

        return groups;
    }

    /// <summary>
    /// CHANGED: luồng 2 dầm trên 1 cột.
    /// M = tâm cột; g = G/2; ⊥ → LocationCurve; xéo → Opening (share hạ tầng).
    /// </summary>
    private static void ExecuteBeamBeamColumn(
        Document doc,
        IList<FamilyInstance> beams,
        FamilyInstance column,
        double totalGapMm,
        HashSet<(int BeamId, int EndIndex)> handledEnds)
    {
        if (!TryGetRectangularColumnPlan(
                column, out XYZ centerM, out _, out _, out _, out _))
            return;

        double halfGapFt = totalGapMm / 304.8 / 2.0;
        double profileExtensionFt = OpeningProfileExtension / 304.8;
        double minAlign = Math.Cos(AngleTolDeg * Math.PI / 180.0);

        foreach (FamilyInstance beam in beams)
        {
            if (beam == null || !beam.IsValidObject)
                continue;

            PrepareBeamEnds(beam);

            if (!TryGetBeamEndNearPoint(
                    beam, centerM, out XYZ endNear, out XYZ otherEnd, out XYZ outDir))
                continue;

            if (beam.Location is not LocationCurve location || location.Curve is not Line)
                continue;

            // CHANGED: đánh dấu đầu gần tâm — không để pha cột–mép đè sau.
            int endIndex = GetBeamEndIndex(beam, endNear);
            if (endIndex >= 0 && handledEnds != null)
                handledEnds.Add((beam.Id.IntegerValue, endIndex));

            XYZ beamDirXY = Flatten(outDir);
            if (beamDirXY == null)
                continue;

            // CHANGED: chọn mặt cột khớp nhất với outDir (dùng chung cho ⊥ / xéo).
            if (!TryGetRectangularColumnFaces(column, out List<ColumnFace> faces))
                continue;

            ColumnFace bestFace = default;
            double bestAlign = -1.0;
            foreach (ColumnFace face in faces)
            {
                double align = Math.Abs(beamDirXY.DotProduct(face.Normal));
                if (align > bestAlign)
                {
                    bestAlign = align;
                    bestFace = face;
                }
            }

            bool isPerpendicularToColumn = bestAlign >= minAlign;

            if (!isPerpendicularToColumn)
            {
                // CHANGED (Beam-Beam xéo only):
                // faceNormal/Dir = mặt cột; faceOrigin = M; gap = G/2
                // → p0-p1 // mép cột; p0-p3/p1-p2 // dầm xéo.
                CreateSkewEndOpeningFromFace(
                    doc, beam, endNear, otherEnd, outDir,
                    bestFace.Normal, bestFace.Dir, centerM,
                    halfGapFt, profileExtensionFt);
                continue;
            }

            // Case vuông góc: endpoint = M - outDir * (G/2).
            XYZ newEnd = SetZ(centerM, endNear.Z) - outDir.Normalize() * halfGapFt;
            XYZ p0 = location.Curve.GetEndPoint(0);
            XYZ p1 = location.Curve.GetEndPoint(1);
            bool nearIsStart = p0.DistanceTo(endNear) <= p1.DistanceTo(endNear);
            if (nearIsStart)
                p0 = newEnd;
            else
                p1 = newEnd;

            if (p0.DistanceTo(p1) < 0.01)
                continue;

            location.Curve = Line.CreateBound(p0, p1);
        }
    }

    /// <summary>
    /// CHANGED: lấy đầu Line gần điểm target (tâm cột) và outDir hướng ra đầu đó.
    /// </summary>
    private static bool TryGetBeamEndNearPoint(
        FamilyInstance beam,
        XYZ target,
        out XYZ endNear,
        out XYZ otherEnd,
        out XYZ outDir)
    {
        endNear = null;
        otherEnd = null;
        outDir = null;

        if (beam.Location is not LocationCurve location || location.Curve is not Line line)
            return false;

        XYZ p0 = line.GetEndPoint(0);
        XYZ p1 = line.GetEndPoint(1);
        XYZ targetXY = SetZ(target, 0);

        if (SetZ(p0, 0).DistanceTo(targetXY) <= SetZ(p1, 0).DistanceTo(targetXY))
        {
            endNear = p0;
            otherEnd = p1;
            outDir = (p0 - p1).Normalize();
        }
        else
        {
            endNear = p1;
            otherEnd = p0;
            outDir = (p1 - p0).Normalize();
        }

        return true;
    }

    /// <summary>CHANGED: endIndex 0 = start, 1 = end; -1 nếu không xác định.</summary>
    private static int GetBeamEndIndex(FamilyInstance beam, XYZ endPoint)
    {
        if (beam?.Location is not LocationCurve location || location.Curve is not Line line)
            return -1;
        XYZ p0 = line.GetEndPoint(0);
        XYZ p1 = line.GetEndPoint(1);
        return p0.DistanceTo(endPoint) <= p1.DistanceTo(endPoint) ? 0 : 1;
    }

    #endregion

    // =====================================================================
    // CHANGED: REGION Three Beams on Column (CheckBox CutWidthLedgeAtPillar)
    // 3 dầm–1 cột: 2 đồng line ⊥ cột (đầu gần tâm) + dầm 3 Line hướng về/xuyên cột.
    // Opening Width Ledge phía có dầm 3. Không sửa region Beam and Wall / Beam and Column.
    // =====================================================================
    #region Three Beams on Column (Width Ledge Checkbox)

    /// <summary>CHANGED: 1 cột + 2 dầm đồng line + 1 dầm thứ 3.</summary>
    private struct ThreeBeamColumnGroup
    {
        public FamilyInstance Column;
        public List<FamilyInstance> CollinearBeams;
        public FamilyInstance ThirdBeam;
    }

    /// <summary>
    /// CHANGED: nhận case 3 dầm–1 cột:
    /// - 2 host đồng line: đầu gần tâm (nearCenterTol) + góc &lt; 3° + ⊥ cột &lt; 3°.
    /// - Dầm thứ 3: Line hướng về/xuyên cột (ưu tiên); nearCenterTol chỉ fallback.
    /// </summary>
    private static List<ThreeBeamColumnGroup> CollectThreeBeamColumnGroups(
        IList<FamilyInstance> beams,
        IList<FamilyInstance> columns,
        HashSet<(int BeamId, int EndIndex)> handledEnds)
    {
        var groups = new List<ThreeBeamColumnGroup>();
        if (beams == null || columns == null)
            return groups;

        double minAlignHostToColumn =
            Math.Cos(CollinearBeamAngleTolDeg * Math.PI / 180.0);

        foreach (FamilyInstance column in columns)
        {
            if (column == null || !column.IsValidObject)
                continue;
            if (!TryGetRectangularColumnPlan(
                    column, out XYZ center, out _, out _, out double halfX, out double halfY))
                continue;
            if (!TryGetRectangularColumnFaces(column, out List<ColumnFace> faces))
                continue;

            // CHANGED: nearCenterTol chỉ bắt buộc cho 2 host đồng line; dầm 3 dùng fallback.
            double nearCenterTolFt =
                Math.Max(halfX, halfY) * 0.5 + SkewOutsideTouchTolMm / 304.8;

            var hostCandidates = new List<(FamilyInstance Beam, XYZ DirXY, int EndIndex)>();
            foreach (FamilyInstance beam in beams)
            {
                if (beam == null || !beam.IsValidObject)
                    continue;
                if (!TryGetBeamEndNearPoint(beam, center, out XYZ endNear, out _, out XYZ outDir))
                    continue;
                if (SetZ(endNear, 0).DistanceTo(SetZ(center, 0)) > nearCenterTolFt)
                    continue;

                int endIndex = GetBeamEndIndex(beam, endNear);
                if (endIndex < 0)
                    continue;
                if (handledEnds != null &&
                    handledEnds.Contains((beam.Id.IntegerValue, endIndex)))
                    continue;

                XYZ dirXY = Flatten(outDir);
                if (dirXY == null)
                    continue;

                hostCandidates.Add((beam, dirXY, endIndex));
            }

            if (hostCandidates.Count < 2)
                continue;

            if (!TryPickCollinearPairPerpToColumn(
                    hostCandidates, faces, minAlignHostToColumn,
                    out FamilyInstance c0, out FamilyInstance c1))
                continue;

            // CHANGED: dầm thứ 3 — Line hướng về/xuyên cột; không bắt đầu gần tâm.
            if (!TryFindThirdBeamForColumn(
                    beams, c0, c1, center, halfX, halfY, nearCenterTolFt, handledEnds,
                    out FamilyInstance third))
                continue;

            groups.Add(new ThreeBeamColumnGroup
            {
                Column = column,
                CollinearBeams = new List<FamilyInstance> { c0, c1 },
                ThirdBeam = third
            });
        }

        return groups;
    }

    /// <summary>
    /// CHANGED: chọn đúng 1 cặp đồng line (góc &lt; 3°) và cả hai ⊥ cột &lt; 3°.
    /// </summary>
    private static bool TryPickCollinearPairPerpToColumn(
        List<(FamilyInstance Beam, XYZ DirXY, int EndIndex)> hostCandidates,
        List<ColumnFace> faces,
        double minAlign,
        out FamilyInstance collinear0,
        out FamilyInstance collinear1)
    {
        collinear0 = null;
        collinear1 = null;

        for (int i = 0; i < hostCandidates.Count; i++)
        for (int j = i + 1; j < hostCandidates.Count; j++)
        {
            XYZ d0 = hostCandidates[i].DirXY;
            XYZ d1 = hostCandidates[j].DirXY;
            if (GetAcuteAngleDegree(d0, d1) > CollinearBeamAngleTolDeg)
                continue;
            if (!IsDirectionPerpendicularToColumn(d0, faces, minAlign))
                continue;
            if (!IsDirectionPerpendicularToColumn(d1, faces, minAlign))
                continue;

            collinear0 = hostCandidates[i].Beam;
            collinear1 = hostCandidates[j].Beam;
            return collinear0 != null && collinear1 != null;
        }

        return false;
    }

    /// <summary>
    /// CHANGED: tìm dầm thứ 3 (không phải 2 host): Line hướng về/xuyên cột; fallback đầu gần tâm.
    /// </summary>
    private static bool TryFindThirdBeamForColumn(
        IList<FamilyInstance> beams,
        FamilyInstance host0,
        FamilyInstance host1,
        XYZ columnCenter,
        double halfX,
        double halfY,
        double nearCenterTolFt,
        HashSet<(int BeamId, int EndIndex)> handledEnds,
        out FamilyInstance third)
    {
        third = null;
        int id0 = host0.Id.IntegerValue;
        int id1 = host1.Id.IntegerValue;
        double bestDist = double.MaxValue;

        foreach (FamilyInstance beam in beams)
        {
            if (beam == null || !beam.IsValidObject)
                continue;
            int id = beam.Id.IntegerValue;
            if (id == id0 || id == id1)
                continue;

            if (!IsBeamLineTowardOrThroughColumn(
                    beam, columnCenter, halfX, halfY, nearCenterTolFt, out double scoreDist))
                continue;

            // Nếu cả 2 đầu đã handled thì bỏ.
            if (handledEnds != null
                && handledEnds.Contains((id, 0))
                && handledEnds.Contains((id, 1)))
                continue;

            if (scoreDist < bestDist)
            {
                bestDist = scoreDist;
                third = beam;
            }
        }

        return third != null;
    }

    /// <summary>
    /// CHANGED: Line dầm hướng về / xuyên cột (chân vuông góc gần footprint).
    /// Fallback: đầu gần tâm ≤ nearCenterTolFt.
    /// </summary>
    private static bool IsBeamLineTowardOrThroughColumn(
        FamilyInstance beam,
        XYZ columnCenter,
        double halfX,
        double halfY,
        double nearCenterTolFt,
        out double scoreDistFt)
    {
        scoreDistFt = double.MaxValue;
        XYZ m = SetZ(columnCenter, 0);
        double allowFt = Math.Max(halfX, halfY) + SkewOutsideTouchTolMm / 304.8;

        if (TryGetBeamLineFootXY(beam, columnCenter, out XYZ foot, out _))
        {
            double dist = foot.DistanceTo(m);
            if (dist <= allowFt)
            {
                scoreDistFt = dist;
                return true;
            }
        }

        // Fallback: đầu gần tâm (số liệu cũ).
        if (!TryGetBeamEndNearPoint(beam, columnCenter, out XYZ endNear, out _, out _))
            return false;
        double endDist = SetZ(endNear, 0).DistanceTo(m);
        if (endDist > nearCenterTolFt)
            return false;
        scoreDistFt = endDist;
        return true;
    }

    /// <summary>CHANGED: chân vuông góc từ tâm cột xuống Line dầm (plan).</summary>
    private static bool TryGetBeamLineFootXY(
        FamilyInstance beam,
        XYZ columnCenter,
        out XYZ footXY,
        out Line line)
    {
        footXY = null;
        line = null;
        if (beam.Location is not LocationCurve location || location.Curve is not Line beamLine)
            return false;

        line = beamLine;
        XYZ a = SetZ(beamLine.GetEndPoint(0), 0);
        XYZ b = SetZ(beamLine.GetEndPoint(1), 0);
        XYZ m = SetZ(columnCenter, 0);
        XYZ ab = b - a;
        double len = ab.GetLength();
        if (len < 1e-9)
            return false;

        XYZ dir = ab / len;
        double t = (m - a).DotProduct(dir);
        footXY = a + dir * t;
        return true;
    }

    private static bool IsDirectionPerpendicularToColumn(
        XYZ beamDirXY,
        List<ColumnFace> faces,
        double minAlign)
    {
        double bestAlign = 0;
        foreach (ColumnFace face in faces)
            bestAlign = Math.Max(bestAlign, Math.Abs(beamDirXY.DotProduct(face.Normal)));
        return bestAlign >= minAlign;
    }

    /// <summary>
    /// CHANGED: (1) cặp đồng line thụt G + Opening Width Ledge;
    /// (2) dầm thứ 3: gap so với mặt stem host gần nhất (⊥ LocationCurve / xéo Opening).
    /// </summary>
    private static void ExecuteThreeBeamColumnLedgeCheckbox(
        Document doc,
        ThreeBeamColumnGroup group,
        double beamBeamGapColumnMm,
        double thirdBeamGapMm,
        HashSet<(int BeamId, int EndIndex)> handledEnds)
    {
        if (group.Column == null || group.CollinearBeams == null || group.CollinearBeams.Count != 2)
            return;
        if (group.ThirdBeam == null || !group.ThirdBeam.IsValidObject)
            return;

        // (1) Cặp đồng line: cùng logic dầm–dầm–cột (G/2 từ tâm).
        if (beamBeamGapColumnMm >= 0)
            ExecuteBeamBeamColumn(
                doc, group.CollinearBeams, group.Column, beamBeamGapColumnMm, handledEnds);

        if (!TryGetRectangularColumnPlan(group.Column, out XYZ centerM, out _, out _, out _, out _))
            return;

        // (1b) Opening Width Ledge trên 2 host (nếu có param).
        if (HostHasAnyWidthLedgeParameters(group.CollinearBeams[0])
            && HostHasAnyWidthLedgeParameters(group.CollinearBeams[1]))
        {
            TryCutCollinearHostsWidthLedgeOpenings(
                doc, group.CollinearBeams, group.ThirdBeam, group.Column, centerM);
        }

        // (2) CHANGED: dầm thứ 3 — thụt/cắt từ mặt stem + gap UI.
        if (thirdBeamGapMm >= 0)
        {
            try
            {
                TryAdjustThirdBeamFromNearestHostStem(
                    doc, group, centerM, thirdBeamGapMm);
            }
            catch
            {
                // Skip dầm 3 nếu fail.
            }
        }

        // CHANGED: chỉ mark đầu dầm 3 gần cột nhóm này — đầu kia còn cho case 2 / cột khác.
        MarkThirdBeamEndsHandled(group.ThirdBeam, centerM, handledEnds);
    }

    /// <summary>CHANGED: tách logic Opening ledge 2 host (cửa sổ sFar [300,500] + mượn).</summary>
    private static void TryCutCollinearHostsWidthLedgeOpenings(
        Document doc,
        IList<FamilyInstance> collinearBeams,
        FamilyInstance thirdBeam,
        FamilyInstance column,
        XYZ centerM)
    {
        var preps = new List<HostLedgeOpeningPrep>(2);
        foreach (FamilyInstance host in collinearBeams)
        {
            if (host == null || !host.IsValidObject)
                continue;
            if (!TryPrepareHostLedgeOpening(
                    host, host.Id.IntegerValue, thirdBeam, column, centerM,
                    out HostLedgeOpeningPrep prep))
                continue;
            preps.Add(prep);
        }

        if (preps.Count == 0)
            return;

        bool[] sFarOk = new bool[preps.Count];
        for (int i = 0; i < preps.Count; i++)
            sFarOk[i] = IsSFarInAcceptWindowFt(preps[i].ComputedSFarFt);

        bool anyOk = false;
        for (int i = 0; i < sFarOk.Length; i++)
            if (sFarOk[i]) anyOk = true;
        if (!anyOk)
            return;

        double? borrowedSFarFt = null;
        for (int i = 0; i < preps.Count; i++)
        {
            if (sFarOk[i])
            {
                borrowedSFarFt = preps[i].ComputedSFarFt;
                break;
            }
        }

        for (int i = 0; i < preps.Count; i++)
        {
            double sFarFt = sFarOk[i]
                ? preps[i].ComputedSFarFt
                : borrowedSFarFt!.Value;
            try
            {
                CreateHostLedgeOpeningFromPrep(doc, preps[i], sFarFt);
            }
            catch
            {
            }
        }
    }

    /// <summary>
    /// CHANGED: dầm thứ 3 — host gần nhất; mặt stem = Center−Ledge;
    /// ⊥ host → LocationCurve; xéo → CreateSkewEndOpeningFromFace (gap = UI).
    /// </summary>
    private static void TryAdjustThirdBeamFromNearestHostStem(
        Document doc,
        ThreeBeamColumnGroup group,
        XYZ columnCenter,
        double gapMm)
    {
        if (!TryGetNearestCollinearHost(
                group.ThirdBeam, group.CollinearBeams, columnCenter, out FamilyInstance host))
            return;
        if (!TryGetStemFaceOnHostForThird(
                host, group.ThirdBeam, columnCenter,
                out XYZ stemOrigin, out XYZ stemNormal, out XYZ hostDirXY))
            return;
        if (!TryGetBeamEndNearPoint(
                group.ThirdBeam, columnCenter, out XYZ endNear, out XYZ otherEnd, out XYZ outDir))
            return;
        if (group.ThirdBeam.Location is not LocationCurve location || location.Curve is not Line)
            return;

        XYZ thirdDirXY = Flatten(outDir);
        if (thirdDirXY == null || hostDirXY == null)
            return;

        double gapFt = gapMm / 304.8;
        PrepareBeamEnds(group.ThirdBeam);

        bool isPerpToHost =
            Math.Abs(GetAcuteAngleDegree(thirdDirXY, hostDirXY) - 90.0) < CollinearBeamAngleTolDeg;

        if (!isPerpToHost)
        {
            // Xéo: Opening — face = stem, gap = UI.
            double profileExtensionFt = OpeningProfileExtension / 304.8;
            CreateSkewEndOpeningFromFace(
                doc, group.ThirdBeam, endNear, otherEnd, outDir,
                stemNormal, hostDirXY, stemOrigin,
                gapFt, profileExtensionFt);
            return;
        }

        // Vuông góc: thụt LocationCurve từ mặt stem + gap (cùng kiểu cột–dầm).
        if (!TryIntersectFace(
                endNear, outDir, stemOrigin, stemNormal, out XYZ facePoint, out _))
            return;

        XYZ newEnd = facePoint - outDir.Normalize() * gapFt;
        XYZ p0 = location.Curve.GetEndPoint(0);
        XYZ p1 = location.Curve.GetEndPoint(1);
        bool nearIsStart = p0.DistanceTo(endNear) <= p1.DistanceTo(endNear);
        if (nearIsStart)
            p0 = new XYZ(newEnd.X, newEnd.Y, p0.Z);
        else
            p1 = new XYZ(newEnd.X, newEnd.Y, p1.Z);

        if (p0.DistanceTo(p1) < 0.01)
            return;

        location.Curve = Line.CreateBound(p0, p1);
    }

    /// <summary>CHANGED: host đồng line có LocationCurve gần điểm đại diện dầm 3 nhất.</summary>
    private static bool TryGetNearestCollinearHost(
        FamilyInstance thirdBeam,
        IList<FamilyInstance> collinearBeams,
        XYZ columnCenter,
        out FamilyInstance nearestHost)
    {
        nearestHost = null;
        if (!TryGetBeamLineFootXY(thirdBeam, columnCenter, out XYZ refPt, out _)
            && !TryGetBeamEndNearPoint(thirdBeam, columnCenter, out refPt, out _, out _))
            return false;

        refPt = SetZ(refPt, 0);
        double best = double.MaxValue;

        foreach (FamilyInstance host in collinearBeams)
        {
            if (host?.Location is not LocationCurve lc || lc.Curve is not Line hostLine)
                continue;

            IntersectionResult projection = hostLine.Project(refPt);
            double dist = projection != null
                ? projection.Distance
                : SetZ(hostLine.GetEndPoint(0), 0).DistanceTo(refPt);

            if (dist < best)
            {
                best = dist;
                nearestHost = host;
            }
        }

        return nearestHost != null;
    }

    /// <summary>
    /// CHANGED: mặt stem trên host gần dầm 3 — origin + normal (towardLedge) + hostDirXY.
    /// </summary>
    private static bool TryGetStemFaceOnHostForThird(
        FamilyInstance host,
        FamilyInstance thirdBeam,
        XYZ columnCenter,
        out XYZ stemOrigin,
        out XYZ stemNormal,
        out XYZ hostDirXY)
    {
        stemOrigin = null;
        stemNormal = null;
        hostDirXY = null;

        if (host.Location is not LocationCurve hostLoc || hostLoc.Curve is not Line hostLine)
            return false;
        if (!TryGetBeamEndNearPoint(host, columnCenter, out XYZ hostEndNear, out XYZ hostOtherEnd, out _))
            return false;

        XYZ awayDir = Flatten(hostOtherEnd - hostEndNear);
        if (awayDir == null)
            return false;

        hostDirXY = awayDir;
        XYZ hostCross = XYZ.BasisZ.CrossProduct(awayDir).Normalize();
        XYZ axisPoint = ProjectPointOntoLineXY(columnCenter, hostLine);

        if (!TryGetThirdBeamSidePointForHost(
                thirdBeam, columnCenter, axisPoint, hostCross, out XYZ sidePoint))
            return false;

        double side = (SetZ(sidePoint, 0) - axisPoint).DotProduct(hostCross);
        if (Math.Abs(side) < 1e-9)
            return false;

        stemNormal = hostCross * Math.Sign(side); // toward ledge / dầm 3
        if (!TryGetWidthLedgeParameters(
                host, stemNormal, hostCross,
                out double widthLedgeFt, out double widthLedgeCenterFt))
            return false;

        double stemOffsetFt = widthLedgeCenterFt - widthLedgeFt;
        if (stemOffsetFt < 1e-9)
            return false;

        stemOrigin = SetZ(axisPoint, 0) + stemNormal * stemOffsetFt;
        return true;
    }

    /// <summary>CHANGED: sFar (feet từ mặt cột) thuộc [300, 500] mm.</summary>
    private static bool IsSFarInAcceptWindowFt(double sFarFt)
    {
        double minFt = LedgeOpeningMinFromColumnFaceMm / 304.8;
        double maxFt = LedgeOpeningMaxFromColumnFaceMm / 304.8;
        return sFarFt >= minFt && sFarFt <= maxFt;
    }

    /// <summary>
    /// CHANGED: chỉ khóa 1 đầu dầm 3 gần tâm cột (start hoặc end).
    /// Không mark cả 2 đầu — đầu còn lại vẫn gom case 2 / cột khác.
    /// </summary>
    private static void MarkThirdBeamEndsHandled(
        FamilyInstance thirdBeam,
        XYZ columnCenter,
        HashSet<(int BeamId, int EndIndex)> handledEnds)
    {
        if (handledEnds == null || thirdBeam == null || !thirdBeam.IsValidObject)
            return;
        if (columnCenter == null)
            return;
        if (!TryGetBeamEndNearPoint(thirdBeam, columnCenter, out XYZ endNear, out _, out _))
            return;

        int endIndex = GetBeamEndIndex(thirdBeam, endNear);
        if (endIndex < 0)
            return;

        handledEnds.Add((thirdBeam.Id.IntegerValue, endIndex));
    }

    /// <summary>CHANGED: dữ liệu chuẩn bị Opening HCN trên 1 host đồng line.</summary>
    private struct HostLedgeOpeningPrep
    {
        public FamilyInstance Host;
        public int HostBeamId;
        public double ComputedSFarFt;
        public XYZ FaceOrigin;
        public XYZ AwayDir;
        public XYZ TowardLedge;
        public XYZ ColumnCenterM;
        public double StemOffsetFt;
        public double DepthFt;
        public double TopZ;
    }

    /// <summary>
    /// CHANGED: chuẩn bị hình học + ComputedSFarFt = max(sMax+10mm, 300mm) từ mặt cột.
    /// Chưa kiểm tra cửa sổ [300,500] và chưa NewOpening.
    /// </summary>
    private static bool TryPrepareHostLedgeOpening(
        FamilyInstance host,
        int hostBeamId,
        FamilyInstance thirdBeam,
        FamilyInstance column,
        XYZ columnCenter,
        out HostLedgeOpeningPrep prep)
    {
        prep = default;
        if (host == null || !host.IsValidObject || host.Id.IntegerValue != hostBeamId)
            return false;
        if (host.Location is not LocationCurve hostLoc || hostLoc.Curve is not Line hostLine)
            return false;
        if (!TryGetBeamEndNearPoint(host, columnCenter, out XYZ hostEndNear, out XYZ hostOtherEnd, out _))
            return false;
        if (!TryGetRectangularColumnPlan(
                column, out XYZ columnCenterPt, out XYZ axisX, out XYZ axisY,
                out double halfX, out double halfY))
            return false;

        XYZ awayDir = Flatten(hostOtherEnd - hostEndNear);
        if (awayDir == null)
            return false;

        XYZ hostCross = XYZ.BasisZ.CrossProduct(awayDir).Normalize();
        XYZ axisPoint = ProjectPointOntoLineXY(columnCenter, hostLine);

        if (!TryGetThirdBeamSidePointForHost(
                thirdBeam, columnCenter, axisPoint, hostCross, out XYZ sidePoint))
            return false;

        double side = (SetZ(sidePoint, 0) - axisPoint).DotProduct(hostCross);
        if (Math.Abs(side) < 1e-9)
            return false;
        XYZ towardLedge = hostCross * Math.Sign(side);

        if (!TryGetWidthLedgeParameters(
                host, towardLedge, hostCross,
                out double widthLedgeFt, out double widthLedgeCenterFt))
            return false;

        double stemOffsetFt = widthLedgeCenterFt - widthLedgeFt;
        if (stemOffsetFt < 1e-9)
            return false;

        double depthFt = widthLedgeFt + LedgeOpeningExtraMm / 304.8;
        double extraFt = LedgeOpeningExtraMm / 304.8;
        double minFarFt = LedgeOpeningMinFromColumnFaceMm / 304.8;
        double colHalf = Math.Max(halfX, halfY);

        if (!TryGetColumnFaceStationAlongDir(
                columnCenterPt, awayDir, axisX, axisY, halfX, halfY, out XYZ faceOrigin))
            return false;

        if (!TryCollectSolidPlanPoints(thirdBeam, out List<XYZ> solidPoints) || solidPoints.Count == 0)
            return false;

        double sMax = double.MinValue;
        bool hasSample = false;
        foreach (XYZ pt in solidPoints)
        {
            XYZ p = SetZ(pt, 0);
            double s = (p - faceOrigin).DotProduct(awayDir);
            double n = (p - axisPoint).DotProduct(towardLedge);
            if (s < -colHalf * 2.0)
                continue;
            if (n < -extraFt)
                continue;

            sMax = Math.Max(sMax, s);
            hasSample = true;
        }

        if (!hasSample)
            return false;

        double sFar = Math.Max(sMax + extraFt, minFarFt);

        BoundingBoxXYZ bbox = host.get_BoundingBox(null);
        if (bbox == null)
            return false;

        prep = new HostLedgeOpeningPrep
        {
            Host = host,
            HostBeamId = hostBeamId,
            ComputedSFarFt = sFar,
            FaceOrigin = faceOrigin,
            AwayDir = awayDir,
            TowardLedge = towardLedge,
            ColumnCenterM = SetZ(columnCenterPt, 0),
            StemOffsetFt = stemOffsetFt,
            DepthFt = depthFt,
            TopZ = bbox.Max.Z
        };
        return true;
    }

    /// <summary>
    /// CHANGED: tạo Opening với sFar đã chọn (tự tính hoặc mượn host kia).
    /// sNear = mirror sFar qua tâm cột M (giữ như cũ).
    /// </summary>
    private static void CreateHostLedgeOpeningFromPrep(
        Document doc,
        HostLedgeOpeningPrep prep,
        double sFarFt)
    {
        double extraFt = LedgeOpeningExtraMm / 304.8;
        XYZ farMid = prep.FaceOrigin + prep.AwayDir * sFarFt;
        XYZ nearMid = prep.ColumnCenterM + prep.ColumnCenterM - farMid;
        double sNear = (nearMid - prep.FaceOrigin).DotProduct(prep.AwayDir);
        if (sFarFt - sNear < extraFt)
            return;

        XYZ p0 = prep.FaceOrigin + prep.AwayDir * sNear + prep.TowardLedge * prep.StemOffsetFt;
        XYZ p3 = prep.FaceOrigin + prep.AwayDir * sNear
                 + prep.TowardLedge * (prep.StemOffsetFt + prep.DepthFt);
        XYZ p1 = prep.FaceOrigin + prep.AwayDir * sFarFt + prep.TowardLedge * prep.StemOffsetFt;
        XYZ p2 = prep.FaceOrigin + prep.AwayDir * sFarFt
                 + prep.TowardLedge * (prep.StemOffsetFt + prep.DepthFt);

        p0 = SetZ(p0, prep.TopZ);
        p1 = SetZ(p1, prep.TopZ);
        p2 = SetZ(p2, prep.TopZ);
        p3 = SetZ(p3, prep.TopZ);

        CurveArray profile = new CurveArray();
        profile.Append(Line.CreateBound(p0, p1));
        profile.Append(Line.CreateBound(p1, p2));
        profile.Append(Line.CreateBound(p2, p3));
        profile.Append(Line.CreateBound(p3, p0));

        doc.Create.NewOpening(prep.Host, profile, Autodesk.Revit.Creation.eRefFace.CenterZ);
    }

    /// <summary>
    /// CHANGED: điểm để chọn phía ledge có dầm thứ 3.
    /// Ưu tiên chân vuông góc tâm cột lên Line; nếu |n| nhỏ thì lấy start/end lệch phía hostCross hơn.
    /// </summary>
    private static bool TryGetThirdBeamSidePointForHost(
        FamilyInstance thirdBeam,
        XYZ columnCenter,
        XYZ hostAxisPoint,
        XYZ hostCross,
        out XYZ sidePoint)
    {
        sidePoint = null;
        if (thirdBeam.Location is not LocationCurve location || location.Curve is not Line line)
            return false;

        XYZ p0 = SetZ(line.GetEndPoint(0), 0);
        XYZ p1 = SetZ(line.GetEndPoint(1), 0);
        XYZ axis = SetZ(hostAxisPoint, 0);

        XYZ best = null;
        double bestAbsN = -1.0;

        if (TryGetBeamLineFootXY(thirdBeam, columnCenter, out XYZ foot, out _))
        {
            double nFoot = Math.Abs((foot - axis).DotProduct(hostCross));
            best = foot;
            bestAbsN = nFoot;
        }

        double n0 = Math.Abs((p0 - axis).DotProduct(hostCross));
        if (n0 > bestAbsN)
        {
            bestAbsN = n0;
            best = p0;
        }

        double n1 = Math.Abs((p1 - axis).DotProduct(hostCross));
        if (n1 > bestAbsN)
        {
            bestAbsN = n1;
            best = p1;
        }

        if (best == null || bestAbsN < 1e-9)
            return false;

        sidePoint = best;
        return true;
    }

    /// <summary>CHANGED: host có bất kỳ bộ Width Ledge (+ Center) / 1 / 2.</summary>
    private static bool HostHasAnyWidthLedgeParameters(FamilyInstance host)
    {
        if (host == null)
            return false;
        if (TryGetDoubleParameter(host, "Width Ledge", out double wl)
            && TryGetDoubleParameter(host, "Width Ledge Center", out double wlc)
            && wl > 1e-9 && wlc > 1e-9)
            return true;
        if (TryGetDoubleParameter(host, "Width Ledge 1", out wl)
            && TryGetDoubleParameter(host, "Width Ledge Center 1", out wlc)
            && wl > 1e-9 && wlc > 1e-9)
            return true;
        if (TryGetDoubleParameter(host, "Width Ledge 2", out wl)
            && TryGetDoubleParameter(host, "Width Ledge Center 2", out wlc)
            && wl > 1e-9 && wlc > 1e-9)
            return true;
        return false;
    }

    /// <summary>CHANGED: điểm trên trục awayDir tại mặt footprint cột (s = 0).</summary>
    private static bool TryGetColumnFaceStationAlongDir(
        XYZ columnCenter,
        XYZ awayDir,
        XYZ axisX,
        XYZ axisY,
        double halfX,
        double halfY,
        out XYZ faceOrigin)
    {
        faceOrigin = null;
        double dx = Math.Abs(awayDir.DotProduct(axisX));
        double dy = Math.Abs(awayDir.DotProduct(axisY));
        double tx = dx > 1e-9 ? halfX / dx : double.MaxValue;
        double ty = dy > 1e-9 ? halfY / dy : double.MaxValue;
        double dist = Math.Min(tx, ty);
        if (double.IsInfinity(dist) || dist > 1e6)
            return false;

        faceOrigin = SetZ(columnCenter, 0) + awayDir * dist;
        return true;
    }

    /// <summary>CHANGED: lấy điểm plan từ Solid dầm thứ 3 (edge + mesh) — dùng bao Opening HCN.</summary>
    private static bool TryCollectSolidPlanPoints(FamilyInstance beam, out List<XYZ> points)
    {
        points = new List<XYZ>();
        Options options = new Options
        {
            ComputeReferences = false,
            IncludeNonVisibleObjects = false,
            DetailLevel = ViewDetailLevel.Fine
        };

        GeometryElement geometry = beam.get_Geometry(options);
        if (geometry == null)
            return false;

        foreach (GeometryObject geometryObject in geometry)
            AccumulateSolidPlanPoints(geometryObject, points);

        return points.Count > 0;
    }

    private static void AccumulateSolidPlanPoints(GeometryObject geometryObject, List<XYZ> points)
    {
        switch (geometryObject)
        {
            case GeometryInstance instance:
            {
                GeometryElement instanceGeometry = instance.GetInstanceGeometry();
                if (instanceGeometry == null)
                    return;
                foreach (GeometryObject nested in instanceGeometry)
                    AccumulateSolidPlanPoints(nested, points);
                break;
            }
            case Solid solid when solid.Volume > 1e-9:
            {
                foreach (Edge edge in solid.Edges)
                {
                    Curve curve = edge.AsCurve();
                    if (curve == null)
                        continue;
                    points.Add(curve.GetEndPoint(0));
                    points.Add(curve.GetEndPoint(1));
                }

                foreach (Face face in solid.Faces)
                {
                    Mesh mesh = face.Triangulate();
                    if (mesh == null)
                        continue;
                    foreach (XYZ vertex in mesh.Vertices)
                        points.Add(vertex);
                }

                break;
            }
        }
    }

    /// <summary>
    /// CHANGED: đọc Width Ledge (+ Center) hoặc Width Ledge 1/2 (+ Center 1/2) theo phía towardLedge.
    /// Chỉ cover 2 family ledge đã gửi.
    /// </summary>
    private static bool TryGetWidthLedgeParameters(
        FamilyInstance host,
        XYZ towardLedge,
        XYZ hostCross,
        out double widthLedgeFt,
        out double widthLedgeCenterFt)
    {
        widthLedgeFt = 0;
        widthLedgeCenterFt = 0;

        if (TryGetDoubleParameter(host, "Width Ledge", out widthLedgeFt)
            && TryGetDoubleParameter(host, "Width Ledge Center", out widthLedgeCenterFt)
            && widthLedgeFt > 1e-9 && widthLedgeCenterFt > 1e-9)
            return true;

        // Family 2 phía: chọn 1 hoặc 2 theo dấu towardLedge · hostCross (đã cùng hệ).
        bool preferSide1 = towardLedge.DotProduct(hostCross) >= 0;
        if (preferSide1)
        {
            if (TryGetDoubleParameter(host, "Width Ledge 1", out widthLedgeFt)
                && TryGetDoubleParameter(host, "Width Ledge Center 1", out widthLedgeCenterFt)
                && widthLedgeFt > 1e-9 && widthLedgeCenterFt > 1e-9)
                return true;
            if (TryGetDoubleParameter(host, "Width Ledge 2", out widthLedgeFt)
                && TryGetDoubleParameter(host, "Width Ledge Center 2", out widthLedgeCenterFt)
                && widthLedgeFt > 1e-9 && widthLedgeCenterFt > 1e-9)
                return true;
        }
        else
        {
            if (TryGetDoubleParameter(host, "Width Ledge 2", out widthLedgeFt)
                && TryGetDoubleParameter(host, "Width Ledge Center 2", out widthLedgeCenterFt)
                && widthLedgeFt > 1e-9 && widthLedgeCenterFt > 1e-9)
                return true;
            if (TryGetDoubleParameter(host, "Width Ledge 1", out widthLedgeFt)
                && TryGetDoubleParameter(host, "Width Ledge Center 1", out widthLedgeCenterFt)
                && widthLedgeFt > 1e-9 && widthLedgeCenterFt > 1e-9)
                return true;
        }

        return false;
    }

    private static bool TryGetDoubleParameter(FamilyInstance instance, string name, out double valueFt)
    {
        valueFt = 0;
        Parameter p = instance.LookupParameter(name);
        if (p != null && p.StorageType == StorageType.Double && p.AsDouble() > 1e-9)
        {
            valueFt = p.AsDouble();
            return true;
        }

        ElementType type = instance.Document.GetElement(instance.GetTypeId()) as ElementType;
        if (type == null)
            return false;

        p = type.LookupParameter(name);
        if (p != null && p.StorageType == StorageType.Double && p.AsDouble() > 1e-9)
        {
            valueFt = p.AsDouble();
            return true;
        }

        return false;
    }

    private static XYZ ProjectPointOntoLineXY(XYZ point, Line line)
    {
        XYZ a = SetZ(line.GetEndPoint(0), 0);
        XYZ b = SetZ(line.GetEndPoint(1), 0);
        XYZ d = (b - a);
        double len = d.GetLength();
        if (len < 1e-9)
            return a;
        d = d / len;
        double t = (SetZ(point, 0) - a).DotProduct(d);
        return a + d * t;
    }

    #endregion

    #region  Beam and Wall
    #region case 3 , beam and wall , beam diagonal wall (!=90 degree) (diagonal : xéo)

    /// <summary>
    /// Case 3: cắt mọi đầu dầm xéo đang giao mặt ngoài tường.
    /// Profile Opening nằm ngang; cạnh cắt song song với tường.
    /// </summary>
    private static bool TryCutSkewBeamByOpening(Document doc, FamilyInstance beam,
        Line beamLine, IList<Wall> walls, double gapFt, double insideTolFt)
    {
        XYZ start = beamLine.GetEndPoint(0);
        XYZ end = beamLine.GetEndPoint(1);
        XYZ beamDir = (end - start).Normalize();
        double searchTolFt = OpeningProfileExtension / 304.8;
        double profileExtensionFt = OpeningProfileExtension / 304.8;
        bool openingCreated = false;

        // Đầu Start: hướng ra ngoài ngược hướng dầm.
        if (TryFindSkewWallAtEnd(start, beamDir.Negate(), walls,
                searchTolFt, insideTolFt, out Wall startWall))
        {
            CreateSkewEndOpening(doc, beam, start, end, beamDir.Negate(),
                startWall, gapFt, profileExtensionFt);
            openingCreated = true;
        }

        // Đầu End: hướng ra ngoài cùng hướng dầm.
        if (TryFindSkewWallAtEnd(end, beamDir.Negate(), walls,
                searchTolFt, insideTolFt, out Wall endWall))
        {
            CreateSkewEndOpening(doc, beam, end, start, beamDir,
                endWall, gapFt, profileExtensionFt);
            openingCreated = true;
        }
        return openingCreated;
    }

    /// <summary>
    /// Tìm tường gần nhất mà một đầu dầm xéo đang giao mặt ngoài.
    /// CHANGED: nhận đầu Line trong tường hoặc ngoài tường cách mặt ngoài &lt;= 10 mm.
    /// </summary>
    private static bool TryFindSkewWallAtEnd(XYZ endPoint, XYZ outDir,
        IList<Wall> walls, double searchTolFt, double insideTolFt, out Wall targetWall)
    {
        targetWall = null;
        XYZ beamDirXY = Flatten(outDir);
        if (beamDirXY == null) {
            return false;
        }

        // CHANGED: sai số ngoài tường 10 mm (feet).
        double outsideTouchTolFt = SkewOutsideTouchTolMm / 304.8;
        double bestDistance = double.MaxValue;

        foreach (Wall wall in walls) {
            if (wall == null || !wall.IsValidObject) continue;
            XYZ wallDir = GetWallDirection(wall);
            XYZ wallNormal = Flatten(wall.Orientation);
            if (wallDir == null || wallNormal == null) continue;

            double angleDeg = GetAcuteAngleDegree(beamDirXY, wallDir);

            // Case 3: góc dầm–tường lệch khỏi 90° từ 0.1 độ trở lên.
            if (Math.Abs(angleDeg - 90.0) < AngleTolDeg) continue;

            // CHANGED: bỏ "chỉ ngoài tường".
            // Nhận khi đầu Line trong tường HOẶC ngoài tường cách mặt ngoài <= 10 mm.
            if (!IsBeamEndMeetingWall(endPoint, wall, insideTolFt, outsideTouchTolFt))
                continue;

            XYZ exteriorOrigin = GetFaceOrigin(wall, exterior: true);
            if (exteriorOrigin == null) continue;
            if (!TryIntersectFace(endPoint, outDir, exteriorOrigin, wallNormal,
                    out XYZ hitPoint, out double t)) continue;
            // CHANGED: cho phép đầu trong tường — tầm tìm theo Width + 10 mm.
            double maxSearchFt = Math.Max(searchTolFt, wall.Width + outsideTouchTolFt);
            if (Math.Abs(t) > maxSearchFt) continue;
            if (!IsPointWithinWallLength(hitPoint, wall, maxSearchFt)) continue;

            double distance = Math.Abs(t);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                targetWall = wall;
            }
        }
        return targetWall != null;

    }

    /// <summary>
    /// CHANGED (Case 3): đầu Line của dầm coi là giao tường khi:
    /// 1) nằm trong bề dày tường, hoặc
    /// 2) nằm ngoài tường nhưng khoảng cách tới mặt ngoài &lt;= outsideTouchTolFt (10 mm).
    /// </summary>
    private static bool IsBeamEndMeetingWall(XYZ endPoint, Wall wall,
        double insideTolFt, double outsideTouchTolFt)
    {
        // 1) Trong tường
        if (IsEndInsideWallThickness(endPoint, wall, insideTolFt))
            return true;

        // 2) Ngoài tường, sát mặt ngoài <= 10 mm
        XYZ n = Flatten(wall.Orientation);
        XYZ exteriorOrigin = GetFaceOrigin(wall, exterior: true);
        if (n == null || exteriorOrigin == null) return false;

        double distToExterior = Math.Abs((endPoint - exteriorOrigin).DotProduct(n));
        return distToExterior <= outsideTouchTolFt;
    }


    /// <summary>
    /// Opening By Face từ tường — CHANGED: ủy quyền sang CreateSkewEndOpeningFromFace (share với cột).
    /// </summary>
    private static Opening CreateSkewEndOpening(Document doc, FamilyInstance beam, XYZ endPoint,
        XYZ otherEnd, XYZ outDir, Wall wall, double gapFt, double profileExtensionFt)
    {
        XYZ wallNormal = Flatten(wall.Orientation);
        XYZ wallDir = GetWallDirection(wall);
        XYZ exteriorOrigin = GetFaceOrigin(wall, exterior: true);

        if (wallNormal == null || wallDir == null || exteriorOrigin == null)
            throw new InvalidOperationException("Cannot determine the exterior wall face.");

        return CreateSkewEndOpeningFromFace(
            doc, beam, endPoint, otherEnd, outDir,
            wallNormal, wallDir, exteriorOrigin, gapFt, profileExtensionFt);
    }

    /// <summary>
    /// CHANGED (share tường/cột): Opening hình bình hành.
    /// Cạnh cắt // mặt đỡ + gap; cạnh bên // Line dầm + Clearance; Width dầm = Solid (GetBeamPlanWidth).
    /// </summary>
    private static Opening CreateSkewEndOpeningFromFace(
        Document doc,
        FamilyInstance beam,
        XYZ endPoint,
        XYZ otherEnd,
        XYZ outDir,
        XYZ faceNormal,
        XYZ faceDir,
        XYZ faceOrigin,
        double gapFt,
        double profileExtensionFt)
    {
        faceNormal = Flatten(faceNormal);
        faceDir = Flatten(faceDir);
        if (faceNormal == null || faceDir == null)
            throw new InvalidOperationException("Cannot determine the support face.");

        double bodySide = Math.Sign((otherEnd - faceOrigin).DotProduct(faceNormal));
        if (bodySide == 0)
            bodySide = Math.Sign(outDir.Negate().DotProduct(faceNormal));
        if (bodySide == 0)
            bodySide = 1.0;

        XYZ cutPlaneOrigin = faceOrigin + faceNormal * bodySide * gapFt;
        if (!TryIntersectFace(
                endPoint, outDir, cutPlaneOrigin, faceNormal,
                out XYZ cutCenter, out _))
            throw new InvalidOperationException("Cannot intersect the beam axis with the opening cut plane.");

        XYZ beamDirXY = Flatten(outDir);
        if (beamDirXY == null)
            throw new InvalidOperationException("Cannot determine the beam direction in plan.");

        XYZ beamCross = XYZ.BasisZ.CrossProduct(beamDirXY).Normalize();
        // GetBeamPlanWidth: đang ưu tiên Solid (param Width đang comment trong hàm đó).
        double beamWidth = GetBeamPlanWidth(beam, beamCross);

        double sideClearanceFt = SkewSideClearanceMm / 304.8;
        double halfSideOffset = beamWidth / 2.0 + sideClearanceFt;

        XYZ p0 = IntersectBeamOffsetWithCutPlane(
            cutCenter, beamDirXY, -beamCross * halfSideOffset, faceNormal);
        XYZ p1 = IntersectBeamOffsetWithCutPlane(
            cutCenter, beamDirXY, beamCross * halfSideOffset, faceNormal);

        double distanceToEnd = Math.Max(0.0, (endPoint - cutCenter).DotProduct(beamDirXY));
        double profileDepth = distanceToEnd + profileExtensionFt;

        BoundingBoxXYZ boundingBox = beam.get_BoundingBox(null);
        if (boundingBox == null)
            throw new InvalidOperationException("Cannot determine the beam top elevation.");

        double topZ = boundingBox.Max.Z;
        p0 = SetZ(p0, topZ);
        p1 = SetZ(p1, topZ);
        XYZ p2 = SetZ(p1 + beamDirXY * profileDepth, topZ);
        XYZ p3 = SetZ(p0 + beamDirXY * profileDepth, topZ);

        CurveArray profile = new CurveArray();
        profile.Append(Line.CreateBound(p0, p1));
        profile.Append(Line.CreateBound(p1, p2));
        profile.Append(Line.CreateBound(p2, p3));
        profile.Append(Line.CreateBound(p3, p0));

        return doc.Create.NewOpening(
            beam,
            profile,
            Autodesk.Revit.Creation.eRefFace.CenterZ);
    }

    /// <summary>
    /// CHANGED: giao đường // Line dầm (đi qua cutCenter + lateralOffset) với mặt cắt // tường.
    /// </summary>
    private static XYZ IntersectBeamOffsetWithCutPlane(XYZ cutCenter,
        XYZ beamDirXY, XYZ lateralOffset, XYZ wallNormal)
    {
        // Điểm trên đường offset: (cutCenter + lateralOffset) + t * beamDirXY
        // Nằm trên mặt cắt: (point - cutCenter) · wallNormal = 0
        double denom = beamDirXY.DotProduct(wallNormal);
        if (Math.Abs(denom) < 1e-9)
            throw new InvalidOperationException("Beam is nearly parallel to the wall cut plane.");

        double t = -lateralOffset.DotProduct(wallNormal) / denom;
        return cutCenter + lateralOffset + beamDirXY * t;
    }


    /// <summary>
    /// CHANGED: lấy bề rộng dầm trên mặt bằng theo 3 tầng:
    /// 1) Parameter Width / b / B (tạm dùng Width; có thể comment case này để ép sang Solid),
    /// 2) Solid geometry chiếu lên beamCross (chuẩn cho profile phức tạp),
    /// 3) AABB thế giới (fallback nghiên cứu — dầm xéo dễ sai).
    /// </summary>
    private static double GetBeamPlanWidth(FamilyInstance beam, XYZ beamCross)
    {
        // // ===== CASE 1: LookupParameter Width / b / B =====
        // // CHANGED: ưu tiên param. Muốn bỏ qua case 1 thì comment khối if bên dưới.
        // if (TryGetBeamWidthFromParameter(beam, out double widthFromParam))
        //     return widthFromParam;

        // ===== CASE 2: Solid profile → max-min chiếu lên beamCross =====
        // CHANGED: đo bao ngoài thật của geometry instance.
        if (TryGetBeamWidthFromSolid(beam, beamCross, out double widthFromSolid))
            return widthFromSolid;

        // // ===== CASE 3: AABB thế giới (fallback / nghiên cứu) =====
        // // CHANGED: giữ thuật toán hộp 8 góc cũ; dầm xéo có thể ra gần chiều dài.
        // return GetBeamWidthFromWorldBoundingBox(beam, beamCross);
        throw new InvalidOperationException("Cannot get beam width from Solid.");
    }

    // /// <summary>
    // /// CHANGED Case 1: đọc bề rộng từ parameter. Thêm tên param ở BeamWidthParameterNames.
    // /// </summary>
    // private static bool TryGetBeamWidthFromParameter(FamilyInstance beam, out double widthFt)
    // {
    //     widthFt = 0.0;
    //     foreach (string name in BeamWidthParameterNames) {
    //         Parameter parameter = beam.LookupParameter(name);
    //         if (parameter == null || parameter.StorageType != StorageType.Double) continue;
    //         double value = parameter.AsDouble();
    //         if (value > 1e-9)
    //         {
    //             widthFt = value;
    //             return true;
    //         }
    //     }
    //
    //     // Thử thêm trên ElementType (một số family để Width ở type).
    //     ElementType? beamType = beam.Document.GetElement(beam.GetTypeId()) as ElementType;
    //     if (beamType == null) return false;
    //
    //     foreach (string name in BeamWidthParameterNames) {
    //         Parameter parameter = beamType.LookupParameter(name);
    //         if (parameter == null || parameter.StorageType != StorageType.Double)
    //             continue;
    //         double value = parameter.AsDouble();
    //         if (value > 1e-9)
    //         {
    //             widthFt = value;
    //             return true;
    //         }
    //     }
    //     return false;
    // }

    /// <summary>
    /// CHANGED Case 2: lấy Solid của Structural Framing, chiếu đỉnh lên beamCross → bề rộng bao ngoài.
    /// </summary>
    private static bool TryGetBeamWidthFromSolid(FamilyInstance beam, XYZ beamCross, out double widthFt)
    {
        widthFt = 0.0;

        Options options = new Options
        {
            ComputeReferences = false,
            IncludeNonVisibleObjects = false,
            DetailLevel = ViewDetailLevel.Fine
        };

        GeometryElement geometry = beam.get_Geometry(options);
        if (geometry == null) return false;

        double min = double.MaxValue;
        double max = double.MinValue;
        bool hasPoint = false;

        foreach (GeometryObject geometryObject in geometry) {
            AccumulateBeamCrossProjection(geometryObject, beamCross, ref min, ref max, ref hasPoint);
        }
        if (!hasPoint) return false;
        widthFt = Math.Max(max - min, 1.0 / 304.8);
        return true;
    }

    /// <summary>CHANGED Case 2 helper: duyệt GeometryInstance / Solid và chiếu điểm lên beamCross.</summary>
    private static void AccumulateBeamCrossProjection(GeometryObject geometryObject, XYZ beamCross,
        ref double min, ref double max, ref bool hasPoint)
    {
        switch (geometryObject) {
            case GeometryInstance instance:
            {
                // GetInstanceGeometry đã gồm transform instance → tọa độ model.
                GeometryElement instanceGeometry = instance.GetInstanceGeometry();
                if (instanceGeometry == null) return;

                foreach (GeometryObject nested in instanceGeometry)
                {
                    AccumulateBeamCrossProjection(nested, beamCross, ref min, ref max, ref hasPoint);
                }
                break;
            }
            case Solid solid when solid.Volume > 1e-9 :
            {
                foreach (Edge edge in solid.Edges) {
                    Curve curve = edge.AsCurve();
                    if (curve == null) continue;

                    AddProjection(curve.GetEndPoint(0), beamCross, ref min, ref max, ref hasPoint);
                    AddProjection(curve.GetEndPoint(1), beamCross, ref min, ref max, ref hasPoint);
                }

                // Bổ sung đỉnh mesh mặt — profile vát/chamfer vẫn bắt được mép ngoài.
                foreach (Face face in solid.Faces) {
                    Mesh mesh = face.Triangulate();
                    if (mesh == null) continue;

                    foreach (XYZ vertex in mesh.Vertices)
                        AddProjection(vertex, beamCross, ref min, ref max, ref hasPoint);
                }
                break;
            }
        }
    }

    // /// <summary>
    // /// CHANGED Case 3: AABB thế giới — 8 góc hộp chiếu lên beamCross.
    // /// Giữ để nghiên cứu; dầm xéo dễ cho kết quả gần chiều dài.
    // /// </summary>
    // private static double GetBeamWidthFromWorldBoundingBox(FamilyInstance beam, XYZ beamCross)
    // {
    //     BoundingBoxXYZ box = beam.get_BoundingBox(null);
    //     if (box == null) return 200.0 / 304.8;
    //
    //     double min = double.MaxValue;
    //     double max = double.MinValue;
    //
    //     for (int ix = 0; ix <= 1; ix++)
    //     for (int iy = 0; iy <= 1; iy++)
    //     for (int iz = 0; iz <= 1; iz++)
    //     {
    //         XYZ localPoint = new XYZ(
    //             ix == 0 ? box.Min.X : box.Max.X,
    //             iy == 0 ? box.Min.Y : box.Max.Y,
    //             iz == 0 ? box.Min.Z : box.Max.Z);
    //         XYZ point = box.Transform.OfPoint(localPoint);
    //         double projection = point.DotProduct(beamCross);
    //         min = Math.Min(min, projection);
    //         max = Math.Max(max, projection);
    //     }
    //
    //     return Math.Max(max - min, 1.0 / 304.8);
    // }

    private static void AddProjection(
        XYZ point,
        XYZ beamCross,
        ref double min,
        ref double max,
        ref bool hasPoint)
    {
        double projection = point.DotProduct(beamCross);
        min = Math.Min(min, projection);
        max = Math.Max(max, projection);
        hasPoint = true;
    }




    /// <summary>Kiểm tra điểm mặt bằng có nằm trong chiều dài tường (có cộng sai số) không.</summary>
    private static bool IsPointWithinWallLength(XYZ point, Wall wall, double toleranceFt)
    {
        if (wall.Location is not LocationCurve location || location.Curve is not Line wallLine)
            return false;
        XYZ wallStart = SetZ(wallLine.GetEndPoint(0),0);
        XYZ wallEnd = SetZ(wallLine.GetEndPoint(1),0);
        XYZ pointXY = SetZ(point, 0);
        XYZ direction = (wallEnd - wallStart).Normalize();
        double wallLength = wallStart.DistanceTo(wallEnd);
        double station = (pointXY - wallStart).DotProduct(direction);

        return station >= -toleranceFt && station <= wallLength + toleranceFt;
    }



    /// <summary>Lấy hướng tường thẳng trên mặt bằng.</summary>
    private static XYZ GetWallDirection(Wall wall)
    {
        if (wall.Location is not LocationCurve location
            || location.Curve is not Line line) { return null; }
        return Flatten(line.GetEndPoint(1) - line.GetEndPoint(0));
    }

    /// <summary>Góc nhọn trên mặt bằng giữa hai hướng, từ 0 đến 90 độ.</summary>
    private static double GetAcuteAngleDegree(XYZ first, XYZ second)
    {
        double dot = Math.Abs(first.Normalize().DotProduct(second.Normalize()));
        dot = Math.Max(-1.0, Math.Min(1.0, dot));
        return Math.Acos(dot) * 180.0 / Math.PI;
    }

    private static XYZ SetZ(XYZ point, double z)
    {
        return new XYZ(point.X, point.Y, z);
    }
    #endregion

    #region case 1 and case 2 , beam and wall , beam perpendicular wall (==90 degree)
    /// <summary>
    /// Nếu đầu dầm vuông góc và (chạm mặt hoặc nằm trong tường) → chỉnh endpoint đúng gapFt.
    /// Case 1 và Case 2 dùng CÙNG công thức: gap x mm so với mặt đã chạm, lùi về thân dầm.
    /// </summary>
    private static void AdjustEndIfTouchWall(ref XYZ endPoint, XYZ outDir, IList<Wall> walls,
        double gapFt, double touchTolFt, double insideTolFt, bool useExteriorFace)
    {
        WallFaceHit? hit = FindTargetWallFace(endPoint, outDir, walls, touchTolFt, insideTolFt, useExteriorFace);
        if (hit == null) {
            return;
        }

        XYZ facePoint = hit.Value.FacePoint;
        double z = endPoint.Z;

        // ===== CHANGED (Case 1 + Case 2): =====
        // Không đẩy sâu vào trong tường.
        // Gap x mm so với mặt đã chạm → lùi về phía thân dầm (giống case sát mặt).
        XYZ newEnd = facePoint - outDir.Normalize() * gapFt;

        // Giữ cao độ Z của đầu dầm gốc
        endPoint = new XYZ(newEnd.X, newEnd.Y, z);
    }

    /// <summary>
    /// Tìm mặt tường đã chạm để neo gap.
    /// - Case 1: đầu dầm sát mặt UI chọn (ngoài/trong)
    /// - Case 2: đầu dầm nằm trong bề dày tường → lấy mặt gần đầu dầm hơn (mặt đã chạm)
    /// </summary>
    private static WallFaceHit? FindTargetWallFace(XYZ endPoint, XYZ outDir, IList<Wall> walls,
        double touchTolFt, double insideTolFt, bool useExteriorFace)
    {
        double minAlign = Math.Cos(AngleTolDeg * Math.PI / 180);

        XYZ beamDirXY = Flatten(outDir);
        if (beamDirXY == null) return null;

        WallFaceHit? best = null;
        double bestDist = double.MaxValue;

        foreach (Wall wall in walls) {
            if (wall == null || !wall.IsValidObject) {
                continue;
            }

            XYZ nExt = Flatten(wall.Orientation);
            if (nExt == null) continue;

            // Vuông góc với tường
            if (Math.Abs(beamDirXY.DotProduct(nExt)) < minAlign) {
                continue;
            }

            bool insideWall = IsEndInsideWallThickness(endPoint, wall, insideTolFt);

            if (insideWall) {
                XYZ originExt = GetFaceOrigin(wall, exterior: true);
                XYZ originInt = GetFaceOrigin(wall, exterior: false);
                if (originExt == null || originInt == null) {
                    continue;
                }

                double distExt = Math.Abs((endPoint - originExt).DotProduct(nExt));
                double distInt = Math.Abs((endPoint - originInt).DotProduct(nExt));

                // Mặt đã chạm = mặt gần đầu dầm hơn
                bool touchedExterior = distExt <= distInt;
                XYZ touchedNormal = touchedExterior ? nExt : nExt.Negate();
                XYZ touchedOrigin = touchedExterior ? originExt : originInt;
                if (!TryIntersectFace(endPoint, outDir, touchedOrigin,
                        touchedNormal, out XYZ fPoint, out double t)) {
                    continue;
                }

                double dist = Math.Abs(t);
                if (dist < bestDist) {
                    bestDist = dist;
                    best = new WallFaceHit { FacePoint = fPoint };
                }

                continue;
            }

            // ----- Case 1: sát mặt theo lựa chọn UI (ngoài/trong) ----
            XYZ faceNormal = useExteriorFace ? nExt : nExt.Negate();
            XYZ originOnFace = GetFaceOrigin(wall, useExteriorFace);
            if (originOnFace == null) continue;

            if (!TryIntersectFace(endPoint, outDir, originOnFace, faceNormal,
                    out XYZ hitPoint, out double t1)) continue;
            if (Math.Abs(t1) > touchTolFt) continue;
            double d1 = Math.Abs(t1);
            if (d1 < bestDist) {
                bestDist = d1;
                best = new WallFaceHit { FacePoint = hitPoint };
            }

        }
        return best;
    }


    /// <summary>
        /// Giao đường đầu dầm (endPoint + t*outDir) với plane mặt tường.
        /// </summary>
        private static bool TryIntersectFace(XYZ endPoint, XYZ outDir,
            XYZ originOnFace, XYZ faceNormal, out XYZ facePoint, out double t)
        {
            facePoint = null;
            t = 0.0;
            Plane plane = Plane.CreateByNormalAndOrigin(faceNormal, originOnFace);
            double denom = outDir.DotProduct(plane.Normal);
            if (Math.Abs(denom) < 1e-9) {
                return false;
            }

            t = (plane.Origin - endPoint).DotProduct(plane.Normal) / denom;
            facePoint = endPoint + outDir * t;
            return true;
        }

        /// <summary>
        /// Case "dầm ăn vào tường":
        /// dist(đầu → mặt ngoài) + dist(đầu → mặt trong) ≈ wall.Width
        /// (điểm nằm giữa / trên hai mặt tường).
        /// </summary>
        private static bool IsEndInsideWallThickness(XYZ endPoint, Wall wall, double tolFt)
        {
            XYZ flatten = Flatten(wall.Orientation);
            if (flatten == null) {
                return false;
            }

            if (wall.Location is not LocationCurve lc || lc.Curve == null) {
                return false;
            }

            XYZ mid = lc.Curve.Evaluate(0.5, true);
            double half = wall.Width / 2.0;
            XYZ originExt = mid + flatten * half;
            XYZ originInt = mid - flatten * half;

            // Khoảng cách vuông góc tới hai mặt (normal nằm ngang)
            double distExt = Math.Abs((endPoint - originExt).DotProduct(flatten));
            double distInt = Math.Abs((endPoint - originInt).DotProduct(flatten));

            // Điểm ngoài tường: distExt + distInt = Width + 2*d > Width
            // Điểm trong / trên mặt: distExt + distInt ≈ Width
            return Math.Abs(distExt + distInt - wall.Width) <= tolFt;
        }

        /// <summary>Tâm tường ± nửa bề dày → điểm trên mặt ngoài hoặc mặt trong.</summary>
        private static XYZ GetFaceOrigin(Wall wall, bool exterior)
        {
            if (wall.Location is not LocationCurve lc || lc.Curve == null) return null;

            XYZ mid = lc.Curve.Evaluate(0.5, true);
            XYZ n = wall.Orientation;
            double half = wall.Width / 2.0;
            return mid + (exterior ? n : -n) * half;
        }

        private static XYZ Flatten(XYZ v)
        {
            XYZ xy = new XYZ(v.X, v.Y, 0);
            if (xy.GetLength() < 1e-9) {
                return null;
            }

            return xy.Normalize();
        }
        #endregion
    #endregion


    #region Beam and Column
    // =====================================================================
    // CHANGED: REGION Beam and Column (cột chữ nhật)
    // Footprint = hình chữ nhật b×h của cột nhìn xuống mặt bằng.
    // =====================================================================
    /// <summary>CHANGED: một mặt đứng của footprint cột chữ nhật trên plan.</summary>
    private struct ColumnFace
    {
        public XYZ Origin;
        public XYZ Normal;
        public XYZ Dir;
        public double HalfLength;
    }

    /// <summary>CHANGED: kết quả tìm mặt cột mà đầu dầm đang gác/giao.</summary>
    private struct ColumnFaceHit
    {
        public XYZ FacePoint;
        public ColumnFace Face;
    }

    /// <summary>
    /// CHANGED: luồng dầm – cột chữ nhật (gác mép).
    /// 1) Xéo → Opening By Face (share CreateSkewEndOpeningFromFace + GetBeamPlanWidth/Solid).
    /// 2) Vuông góc → lùi LocationCurve từ mép footprint + gap.
    /// CHANGED: bỏ qua đầu đã handled (tường / dầm–dầm–cột).
    /// </summary>
    private static void ExecuteBeamColumn(
        Document doc,
        IList<FamilyInstance> beams,
        IList<FamilyInstance> columns,
        double gapMm,
        HashSet<(int BeamId, int EndIndex)> handledEnds)
    {
        double gapFt = gapMm / 304.8;
        double touchTolFt = TouchTolMm / 304.8;

        foreach (FamilyInstance beam in beams)
        {
            if (beam == null || !beam.IsValidObject)
                continue;

            PrepareBeamEnds(beam);

            if (beam.Location is not LocationCurve location)
                continue;
            if (location.Curve is not Line line)
                continue;

            int beamId = beam.Id.IntegerValue;
            // CHANGED: skip từng đầu đã xử lý ở pha trước.
            bool skip0 = handledEnds != null && handledEnds.Contains((beamId, 0));
            bool skip1 = handledEnds != null && handledEnds.Contains((beamId, 1));
            if (skip0 && skip1)
                continue;

            XYZ p0 = line.GetEndPoint(0);
            XYZ p1 = line.GetEndPoint(1);
            XYZ dir = (p1 - p0).Normalize();

            // Case xéo cột — từng đầu
            bool open0 = !skip0 && TryCutSkewBeamByColumnOpeningAtEnd(
                doc, beam, p0, p1, dir.Negate(), columns, gapFt, touchTolFt);
            bool open1 = !skip1 && TryCutSkewBeamByColumnOpeningAtEnd(
                doc, beam, p1, p0, dir, columns, gapFt, touchTolFt);

            // Case vuông góc — chỉ đầu chưa Opening và chưa handled
            bool movedLocation = false;
            if (!skip0 && !open0)
            {
                XYZ before = p0;
                AdjustEndIfTouchColumn(ref p0, -dir, columns, gapFt, touchTolFt);
                movedLocation |= !p0.IsAlmostEqualTo(before);
            }

            if (!skip1 && !open1)
            {
                XYZ before = p1;
                AdjustEndIfTouchColumn(ref p1, dir, columns, gapFt, touchTolFt);
                movedLocation |= !p1.IsAlmostEqualTo(before);
            }

            if (!movedLocation)
                continue;
            if (p0.DistanceTo(p1) < 0.01)
                continue;

            location.Curve = Line.CreateBound(p0, p1);
        }
    }

    /// <summary>
    /// CHANGED: Opening xéo tại MỘT đầu dầm giao cột (tách từ TryCutSkewBeamByColumnOpening
    /// để skip theo endIndex).
    /// </summary>
    private static bool TryCutSkewBeamByColumnOpeningAtEnd(
        Document doc,
        FamilyInstance beam,
        XYZ endPoint,
        XYZ otherEnd,
        XYZ outDir,
        IList<FamilyInstance> columns,
        double gapFt,
        double touchTolFt)
    {
        if (!TryFindColumnFaceAtEnd(
                endPoint, outDir, columns, touchTolFt,
                requireSkew: true, out ColumnFaceHit hit))
            return false;

        double profileExtensionFt = OpeningProfileExtension / 304.8;
        CreateSkewEndOpeningFromFace(
            doc, beam, endPoint, otherEnd, outDir,
            hit.Face.Normal, hit.Face.Dir, hit.Face.Origin,
            gapFt, profileExtensionFt);
        return true;
    }

    /// <summary>
    /// CHANGED: dầm ⊥ mặt cột — lùi endpoint từ mép footprint vào thân dầm đúng gap.
    /// </summary>
    private static void AdjustEndIfTouchColumn(
        ref XYZ endPoint,
        XYZ outDir,
        IList<FamilyInstance> columns,
        double gapFt,
        double touchTolFt)
    {
        if (!TryFindColumnFaceAtEnd(endPoint, outDir, columns, touchTolFt,
                requireSkew: false, out ColumnFaceHit hit))
            return;

        double z = endPoint.Z;
        XYZ newEnd = hit.FacePoint - outDir.Normalize() * gapFt;
        endPoint = new XYZ(newEnd.X, newEnd.Y, z);
    }

    /// <summary>
    /// CHANGED: tìm mặt footprint cột chữ nhật gần đầu dầm.
    /// requireSkew=true → case xéo; false → case vuông góc (beam // normal mặt).
    /// </summary>
    private static bool TryFindColumnFaceAtEnd(
        XYZ endPoint,
        XYZ outDir,
        IList<FamilyInstance> columns,
        double touchTolFt,
        bool requireSkew,
        out ColumnFaceHit bestHit)
    {
        bestHit = default;
        XYZ beamDirXY = Flatten(outDir);
        if (beamDirXY == null)
            return false;

        double minAlign = Math.Cos(AngleTolDeg * Math.PI / 180.0);
        double bestDistance = double.MaxValue;
        bool found = false;

        foreach (FamilyInstance column in columns)
        {
            if (column == null || !column.IsValidObject)
                continue;
            if (!TryGetRectangularColumnFaces(column, out List<ColumnFace> faces))
                continue;

            foreach (ColumnFace face in faces)
            {
                double align = Math.Abs(beamDirXY.DotProduct(face.Normal));
                double angleToEdge = GetAcuteAngleDegree(beamDirXY, face.Dir);
                bool isPerpendicular = align >= minAlign;
                bool isSkew = Math.Abs(angleToEdge - 90.0) >= AngleTolDeg;

                if (requireSkew)
                {
                    if (!isSkew)
                        continue;
                }
                else
                {
                    if (!isPerpendicular)
                        continue;
                }

                if (!IsBeamEndMeetingColumnFace(endPoint, column, face, touchTolFt))
                    continue;

                if (!TryIntersectFace(
                        endPoint, outDir, face.Origin, face.Normal,
                        out XYZ hitPoint, out double t))
                    continue;

                // Tầm tìm: nửa cạnh cột + buffer.
                double maxSearch = Math.Max(touchTolFt, face.HalfLength * 2.0 + touchTolFt);
                if (Math.Abs(t) > maxSearch)
                    continue;

                // Hit phải nằm trên đoạn cạnh footprint.
                double along = (hitPoint - face.Origin).DotProduct(face.Dir);
                if (Math.Abs(along) > face.HalfLength + touchTolFt)
                    continue;

                double distance = Math.Abs(t);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestHit = new ColumnFaceHit { FacePoint = hitPoint, Face = face };
                    found = true;
                }
            }
        }

        return found;
    }

    /// <summary>
    /// CHANGED: đầu Line giao cột khi nằm trong footprint hoặc cách mặt cột &lt;= touchTol.
    /// </summary>
    private static bool IsBeamEndMeetingColumnFace(
        XYZ endPoint,
        FamilyInstance column,
        ColumnFace face,
        double touchTolFt)
    {
        if (TryGetRectangularColumnPlan(
                column, out XYZ center, out XYZ axisX, out XYZ axisY,
                out double halfX, out double halfY))
        {
            XYZ d = SetZ(endPoint, 0) - SetZ(center, 0);
            double lx = d.DotProduct(axisX);
            double ly = d.DotProduct(axisY);
            if (Math.Abs(lx) <= halfX + touchTolFt && Math.Abs(ly) <= halfY + touchTolFt)
                return true;
        }

        double distToFace = Math.Abs((endPoint - face.Origin).DotProduct(face.Normal));
        return distToFace <= touchTolFt;
    }

    /// <summary>
    /// CHANGED: 4 mặt footprint cột chữ nhật trên plan (phase 1 — chưa hỗ trợ cột tròn).
    /// </summary>
    private static bool TryGetRectangularColumnFaces(
        FamilyInstance column,
        out List<ColumnFace> faces)
    {
        faces = new List<ColumnFace>();
        if (!TryGetRectangularColumnPlan(
                column, out XYZ center, out XYZ axisX, out XYZ axisY,
                out double halfX, out double halfY))
            return false;

        faces.Add(new ColumnFace
        {
            Origin = center + axisX * halfX,
            Normal = axisX,
            Dir = axisY,
            HalfLength = halfY
        });
        faces.Add(new ColumnFace
        {
            Origin = center - axisX * halfX,
            Normal = axisX.Negate(),
            Dir = axisY,
            HalfLength = halfY
        });
        faces.Add(new ColumnFace
        {
            Origin = center + axisY * halfY,
            Normal = axisY,
            Dir = axisX,
            HalfLength = halfX
        });
        faces.Add(new ColumnFace
        {
            Origin = center - axisY * halfY,
            Normal = axisY.Negate(),
            Dir = axisX,
            HalfLength = halfX
        });

        return true;
    }

    /// <summary>
    /// CHANGED: tâm + trục local + nửa bề rộng/sâu cột chữ nhật (param b/h/Width/Depth).
    /// </summary>
    private static bool TryGetRectangularColumnPlan(
        FamilyInstance column,
        out XYZ center,
        out XYZ axisX,
        out XYZ axisY,
        out double halfX,
        out double halfY)
    {
        center = null;
        axisX = null;
        axisY = null;
        halfX = 0;
        halfY = 0;

        if (column.Location is not LocationPoint locationPoint)
            return false;

        center = locationPoint.Point;
        Transform transform = column.GetTransform();
        axisX = Flatten(transform.BasisX);
        axisY = Flatten(transform.BasisY);
        if (axisX == null || axisY == null)
            return false;

        if (!TryGetColumnHalfSizes(column, out halfX, out halfY))
            return false;

        return halfX > 1e-9 && halfY > 1e-9;
    }

    /// <summary>CHANGED: đọc nửa kích thước cột chữ nhật từ instance/type parameters.</summary>
    private static bool TryGetColumnHalfSizes(
        FamilyInstance column,
        out double halfX,
        out double halfY)
    {
        halfX = 0;
        halfY = 0;

        double width = GetFirstPositiveParameter(
            column, "b", "B", "Width", "WIDTH");
        double depth = GetFirstPositiveParameter(
            column, "h", "H", "Depth", "DEPTH", "d", "D");

        // Fallback: nếu chỉ có một cạnh, dùng Solid chiếu tạm theo local X/Y.
        if (width <= 1e-9 || depth <= 1e-9)
        {
            if (!TryGetColumnSizeFromSolid(column, out width, out depth))
                return false;
        }

        halfX = width / 2.0;
        halfY = depth / 2.0;
        return true;
    }

    private static double GetFirstPositiveParameter(FamilyInstance instance, params string[] names)
    {
        foreach (string name in names)
        {
            Parameter p = instance.LookupParameter(name);
            if (p != null && p.StorageType == StorageType.Double && p.AsDouble() > 1e-9)
                return p.AsDouble();
        }

        ElementType type = instance.Document.GetElement(instance.GetTypeId()) as ElementType;
        if (type == null)
            return 0;

        foreach (string name in names)
        {
            Parameter p = type.LookupParameter(name);
            if (p != null && p.StorageType == StorageType.Double && p.AsDouble() > 1e-9)
                return p.AsDouble();
        }

        return 0;
    }

    /// <summary>
    /// CHANGED fallback: ước lượng b×h cột từ Solid chiếu lên local X/Y (không dùng AABB thế giới).
    /// </summary>
    private static bool TryGetColumnSizeFromSolid(
        FamilyInstance column,
        out double width,
        out double depth)
    {
        width = 0;
        depth = 0;

        Transform transform = column.GetTransform();
        XYZ axisX = Flatten(transform.BasisX);
        XYZ axisY = Flatten(transform.BasisY);
        if (axisX == null || axisY == null)
            return false;

        if (!TryGetBeamWidthFromSolid(column, axisX, out width))
            return false;
        if (!TryGetBeamWidthFromSolid(column, axisY, out depth))
            return false;

        return width > 1e-9 && depth > 1e-9;
    }

    #endregion
}

