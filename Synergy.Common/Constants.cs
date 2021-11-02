using System;
using System.Collections.Generic;
using System.Text;

namespace Synergy.Common
{
    public static class Constants
    {
#pragma warning disable CA1034 // Nested types should not be visible
        public static class User
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public static Guid SystemUserId { get; } = new Guid("00000000-0000-0000-0000-000000000001");

            public static string SystemUserName { get; } = "System User";
        }
    }
}
