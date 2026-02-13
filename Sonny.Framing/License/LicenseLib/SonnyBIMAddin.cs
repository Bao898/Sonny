// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace SonnyBIM
{
    public class SonnyBIMAddin
    {
        public SonnyBIMAddin(string nameOfAddin, string version,
            double priceOneYearDola,double priceOneYearVND)
        {
            NameOfAddin = nameOfAddin;
            //Version = version;
            //PriceOneYearDola = priceOneYearDola;
            //PriceOneYearVND = priceOneYearVND;
        }

        [Obfuscation]
        public string NameOfAddin { get; set; }
        public string Version { get; set; }
        //public double PriceOneYearDola { get; set; }
        //public double PriceOneYearVND { get; set; }
    }
}

