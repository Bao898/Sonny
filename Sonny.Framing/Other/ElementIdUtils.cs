// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace SonnyBIM;

public static class ElementIdUtils
{
    public static long GetValue( this ElementId elementId )
    {
#if ALB_R23 || ALB_R22 || ALB_R21
      return elementId.IntegerValue ;
#else
        return elementId.Value ;
#endif
    }
}
