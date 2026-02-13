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
    }
}


