// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

#region Namespaces

#endregion

using System;
using System.Collections.Generic;

namespace SonnyBIM
{
    /// <summary>
    /// SonnyBIMAllAddins - Q8fdaa690396dbff69bab7eec947f57fc
    /// </summary>
    public class Q8fdaa690396dbff69bab7eec947f57fc
    {
        /// <summary>
        /// AllAddins - A78ebb0f03fa90317c36182666ec83d4 : Danh sách tất cả các Add-ins
        /// </summary>
        public List<SonnyBIMAddin> A78ebb0f03fa90317c36182666ec83d4 { get; set; }

        #region BƯỚC 1: Thêm Tên Add-in, Version, Giá $, Giá VNĐ

        /*
         TÊN ADD-INS - VERSION - GIÁ 1 NĂM $ - GIÁ 1 NĂM VNĐ
         Tên trong AllAddins phải phù hợp quy tắc đặt tên: không có dấu /,*,...
         */

        /// <summary>
        /// Tên của một Add-in được bán. Tuple&lt;Tên Add-in, Version, Giá $, Giá VNĐ&gt;
        /// Item1: Tên Add-in;
        /// Item2: Version;
        /// Item3: Giá $;
        /// Item4: Giá VNĐ;
        /// </summary>
        public static readonly Tuple<string, string, double, double> AllSonnyBIM
            = new("ALL SONNY BIM TOOLS", "1.0.0", 0, 0);

        public static readonly Tuple<string, string, double, double> AllSonnyBIMSTR
            = new("ALL SONNY BIM - STR", "1.0.0", 0, 0);

        public static readonly Tuple<string, string, double, double> AllReinforcementTools
            = new("ALL REINFORCEMENT TOOLS",
                "1.0.0", 0, 0);

        public static readonly Tuple<string, string, double, double> AllQSTools
            = new("ALL QUANTITY SURVEY TOOLS", "1.0.0", 0, 0);

        public static readonly Tuple<string, string, double, double> AllGeneralTools
            = new("ALL GENERAL TOOLS", "1.0.0", 0, 0);

        public static readonly Tuple<string, string, double, double> AllInfrastructureTools
            = new("ALL INFRASTRUCTURE TOOLS", "1.0.0", 0, 0);



        public static readonly Tuple<string, string, double, double> FormworkArea = new("Formwork Area", "1.0.0", 0,
            0);

        public static readonly Tuple<string, string, double, double> AutoJoin = new("Auto Join", "1.0.1", 0, 0);

        public static readonly Tuple<string, string, double, double> ModelFromAcad
            = new("Model From AutoCAD", "1.0.0", 0, 0);

        public static readonly Tuple<string, string, double, double> BeamRebar
            = new("Beam Rebar",
                "1.0.0", 0, 0);

        public static readonly Tuple<string, string, double, double> ColumnWallRebar
            = new("Column/Wall Rebar",
                "1.0.0", 0, 0);

        public static readonly Tuple<string, string, double, double> FoundationRebar
            = new("Foundation Rebar",
                "1.0.0", 0, 0);

        public static readonly Tuple<string, string, double, double> RebarShape2D
            = new("Rebar Shape 2D",
                "1.0.0", 0, 0);

        public static readonly Tuple<string, string, double, double> ScheduleRebar
            = new("Rebar Schedule", "1.0.0", 0, 0);

        public static readonly Tuple<string, string, double, double> AutoDimension
            = new("Auto Dimension", "1.0.0", 0, 0);



        public static readonly Tuple<string, string, double, double> Free
            = new Tuple<string, string, double, double>("Free Tools",
                "1.0.0", 0, 0);



        #endregion BƯỚC 1: Thêm Tên Add-in, Version, Giá $, Giá VNĐ

        #region BƯỚC 2: Đặt biệt danh cho Tên của Add-in mới thêm

        /// <summary>
        /// Dictionary&lt; Tên Add-in, Biệt danh &gt;
        /// Đặt biệt danh cho tools.Muốn đặt gì cũng được,
        /// nhưng biệt danh ở version sau phải giống version trước đã phát hành và phải khác nhau
        /// </summary>
        // public static Dictionary<string, string> AllToolsPair = new()
        // {
        //     // {Free.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 0")},
        //     {AllSonnyBIM.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 1")},
        //     {AllSonnyBIMSTR.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 2")},
        //     {AllReinforcementTools.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 3")},
        //     {AllQSTools.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 4")},
        //     {AllInfrastructureTools.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 5")},
        //     {AllGeneralTools.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 10")},
        //
        //     {FormworkArea.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 6")},
        //     {AutoJoin.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 8")},
        //     {ModelFromAcad.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 9")},
        //     {BeamRebar.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 11")},
        //     {ColumnWallRebar.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 12")},
        //     {FoundationRebar.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 13")},
        //     {RebarShape2D.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 14")},
        //     {ScheduleRebar.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 7")},
        //     {AutoDimension.Item1, Kx8c94bae6204254a5d64622afb93.Abcxyz111111111111222222222222222222xxxxxxxxxxxx("AlphaV2 15")},
        // };

        #endregion

        #region BƯỚC 3: Thêm Add-in mới thêm vào danh sách AllAddins

        public Q8fdaa690396dbff69bab7eec947f57fc()
        {
            A78ebb0f03fa90317c36182666ec83d4 = new List<SonnyBIMAddin>()
            {
                new(AllSonnyBIM.Item1,AllSonnyBIM.Item2,AllSonnyBIM.Item3,AllSonnyBIM.Item4),
                new (AllSonnyBIMSTR.Item1,AllSonnyBIMSTR.Item2,AllSonnyBIMSTR.Item3,AllSonnyBIMSTR.Item4),
                new (AllReinforcementTools.Item1,AllReinforcementTools.Item2,AllReinforcementTools.Item3,AllReinforcementTools.Item4),
                new (AllQSTools.Item1,AllQSTools.Item2,AllQSTools.Item3,AllQSTools.Item4),
                new (AllInfrastructureTools.Item1,AllInfrastructureTools.Item2,AllInfrastructureTools.Item3,AllInfrastructureTools.Item4),
                new (AllGeneralTools.Item1,AllGeneralTools.Item2,AllGeneralTools.Item3,AllGeneralTools.Item4),

                new (FormworkArea.Item1,FormworkArea.Item2,FormworkArea.Item3,FormworkArea.Item4),
                new (AutoJoin.Item1,AutoJoin.Item2,AutoJoin.Item3,AutoJoin.Item4),
                new (ModelFromAcad.Item1,ModelFromAcad.Item2,ModelFromAcad.Item3,ModelFromAcad.Item4),

                new (BeamRebar.Item1,BeamRebar.Item2,BeamRebar.Item3,BeamRebar.Item4),
                new (ColumnWallRebar.Item1,ColumnWallRebar.Item2,ColumnWallRebar.Item3,ColumnWallRebar.Item4),
                new (FoundationRebar.Item1,FoundationRebar.Item2,FoundationRebar.Item3,FoundationRebar.Item4),
                new (RebarShape2D.Item1,RebarShape2D.Item2, RebarShape2D.Item3,RebarShape2D.Item4),
                new (ScheduleRebar.Item1,ScheduleRebar.Item2, ScheduleRebar.Item3,ScheduleRebar.Item4),
                new (AutoDimension.Item1,AutoDimension.Item2, AutoDimension.Item3,AutoDimension.Item4),

            };
        }

        #endregion
    }
}

