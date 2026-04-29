// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Parameter = Autodesk.Revit.DB.Parameter ;
using MessageBox = System.Windows.Forms.MessageBox ;

namespace SonnyBIM
{
    public static class FamilyUtils
    {
        public static List<FamilySymbol> GetAllFamilySymbol( this Family family )
        {
            if ( family == null )
                return new List<FamilySymbol>() ;
            List<FamilySymbol> familySymbols = new List<FamilySymbol>() ;

            foreach ( var familySymbolId in family.GetFamilySymbolIds() ) {
                familySymbols.Add( family.Document.GetElement( familySymbolId ) as FamilySymbol ) ;
            }

            return familySymbols.OrderBy( symbol => symbol.Name ).ToList() ;
        }

        /// <summary>
        /// Lấy về hoặc tạo ra một FamilySymbol của một Family Cột Tròn
        /// </summary>
        /// <returns></returns>
        public static FamilySymbol GetFamilySymbolCircleColumn(Family family, double diameter, string diameterPara)
        {
            List<FamilySymbol> allFamilySymbol = family.GetAllFamilySymbol();
            foreach (FamilySymbol familySymbol in allFamilySymbol) {
                Parameter diameterParaTemp = familySymbol.LookupParameter(diameterPara);
                double diameterParaValue = ParameterUtils.GetDouble(diameterParaTemp.GetValue());

                if (Math.Abs(diameterParaValue - diameter) < 1e-4) {
                    return familySymbol;
                }
            }
            // làm tròn đến hàng đơn vị, ví dụ: 2995.5 -> 2996
            double sectionX = Math.Round(SonnyBIMUnitUtils.FeetToMm(diameter), 0);
            string name = string.Concat("D" + sectionX);

            if (name.Equals("D0") || Math.Abs(sectionX) < SonnyBIMConstraint.Tolerance) {
                return null;
            }

            FamilySymbol result = null;
            ElementType s1 = allFamilySymbol[0].Duplicate(name);
            s1.LookupParameter(diameterPara)?.Set(diameter);
            result = s1 as FamilySymbol;
            return result;
        }
    }
}


