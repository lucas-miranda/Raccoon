using System;
using System.Collections.Generic;

namespace Raccoon {
    public static class Extensions {
        #region Enums

        public static List<Enum> GetFlagValues(this Enum enumFlags) {
            List<Enum> separatedFlagValues = new List<Enum>();
            int enumFlagsAsNumber = Convert.ToInt32(enumFlags), enumSize = System.Runtime.InteropServices.Marshal.SizeOf(Enum.GetUnderlyingType(enumFlags.GetType()));
            for (int i = 0; i < 8 * enumSize; i++) {
                int bitValue = 1 << i;
                if ((enumFlagsAsNumber & bitValue) == 0) {
                    continue;
                }

                separatedFlagValues.Add((Enum) Enum.ToObject(enumFlags.GetType(), bitValue));
            }

            return separatedFlagValues;
        }

        #endregion Enums
    }
}
