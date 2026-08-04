// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Autodesk.Revit.UI;

namespace SonnyBIM;

public class AdjustStrBeamViewModel : ViewModelBaseNew
{
    string _languageCode = LanguageData.GetLanguageSetting();

    private double _beamGapWallMm = 20;
    private double _beamGapColumnMm = 20;
    private double _beamBeamGapColumnMm = 20;
    // CHANGED: gap dầm thứ 3 so với mặt stem (mm).
    private double _beamBeamPerpendicularGapMm = 20;
    // CHANGED: CheckBox — 3 dầm/1 cột: thụt cặp đồng line + cắt Width Ledge.
    private bool _cutWidthLedgeAtPillar = true;

    /// <summary>CHANGED: Gap dầm–tường (mm).</summary>
    [Obfuscation]
    public double BeamGapWallMm {
        get => _beamGapWallMm;
        set {_beamGapWallMm = value; OnPropertyChanged();}
    }

    /// <summary>CHANGED: Gap dầm–cột (mm).</summary>
    [Obfuscation]
    public double BeamGapColumnMm {
        get => _beamGapColumnMm;
        set {_beamGapColumnMm = value; OnPropertyChanged();}
    }

    /// <summary>CHANGED: Gap dầm–cột gác mép (mm).</summary>
    /// /// <summary>
    /// CHANGED: Tổng gap G (mm) giữa 2 đầu dầm trên 1 cột (đồng trục / chữ V: mỗi bên G/2 từ tâm).
    /// </summary>
    [Obfuscation]
    public double BeamBeamGapColumnMm
    {
        get => _beamBeamGapColumnMm;
        set { _beamBeamGapColumnMm = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// CHANGED: Gap dầm thứ 3 (⊥/xéo) so với mặt stem host gần nhất (mm).
    /// </summary>
    [Obfuscation]
    public double BeamBeamPerpendicularGapMm
    {
        get => _beamBeamPerpendicularGapMm;
        set
        {
            _beamBeamPerpendicularGapMm = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// CHANGED: CheckBox ON = case 3 dầm–1 cột (ledge + gap dầm 3). OFF = không chạy gì.
    /// </summary>
    [Obfuscation]
    public bool CutWidthLedgeAtPillar
    {
        get => _cutWidthLedgeAtPillar;
        set
        {
            _cutWidthLedgeAtPillar = value;
            OnPropertyChanged();
        }
    }

    // true = mặt ngoài, false = mặt trong (theo Wall.Orientation)
    [Obfuscation]
    public bool UseExteriorFace { get; set; } = true;

    public List<FamilyInstance> Beams { get; set; }
    public List<Wall> Walls { get; set; }
    // CHANGED: danh sách cột đã chọn.
    public List<FamilyInstance> Columns { get; }

    public AdjustStrBeamViewModel(UIDocument uidoc, List<FamilyInstance> beams,
        List<Wall> walls, List<FamilyInstance> columns)
    {
        Beams = beams;
        Walls = walls;
        Columns = columns;
        UiDoc = uidoc;
        Doc = uidoc.Document;
    }

    public void SaveSetting()
    {
        throw new NotImplementedException();
    }
}
