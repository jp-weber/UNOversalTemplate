// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//https://github.com/CommunityToolkit/WindowsCommunityToolkit/blob/c8f76d072df53d3622fb5440d63afb06cb9e7a10/Microsoft.Toolkit.Uwp/Structures/OSVersion.cs#L10

namespace UNOversal.Helpers
{
    public struct OSVersion
    {
        //
        // Summary:
        //     Value describing major version
        public ushort Major;

        //
        // Summary:
        //     Value describing minor version
        public ushort Minor;

        //
        // Summary:
        //     Value describing build
        public ushort Build;

        //
        // Summary:
        //     Value describing revision
        public ushort Revision;

        //
        // Summary:
        //     Converts OSVersion to string
        //
        // Returns:
        //     Major.Minor.Build.Revision as a string
        public override string ToString()
        {
            return $"{Major}.{Minor}.{Build}.{Revision}";
        }
    }
}
