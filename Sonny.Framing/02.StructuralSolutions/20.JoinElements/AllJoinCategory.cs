// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace SonnyBIM
{
    internal static class AllJoinCategory
    {

        #region All Category will choice to Auto Join

        /// <summary>
        /// Tên xuất hiện ở Data grid
        /// </summary>
        internal const string Beam = "Structural Framing";

        /// <summary>
        /// Tên xuất hiện ở Data grid
        /// </summary>
        internal const string StructuralColumn = "Structural Column";

        #endregion All Category will choice to Auto Join


        #region Category Id

        /// <summary>
        /// (int)BuiltInCategory.OST_StructuralFraming
        /// </summary>
        internal static int BeamCategoryId { get; set; } = (int)BuiltInCategory.OST_StructuralFraming;

        /// <summary>
        /// (int)BuiltInCategory.OST_StructuralColumns
        /// </summary>
        internal static int StructuralColumnCategoryId { get; set; } = (int)BuiltInCategory.OST_StructuralColumns;

        #endregion Category Id


        public static List<string> AllCategoryPriority = new List<string>()
        {
            Beam,
            StructuralColumn
        };

        public static List<string> AllCutCategory = new List<string>()
        {
            StructuralColumn
        };



    }

}

