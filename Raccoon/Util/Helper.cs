using System.Collections.Generic;
using Raccoon.Graphics;

namespace Raccoon.Util {
    public static class Helper {
        public static int Index(int index, int count) {
            return index < 0 ? count - 1 + (index % count) : index % count;
        }

        public static T At<T>(IList<T> list, int index) {
            return list[Index(index, list.Count)];
        }

        public static bool EqualsPermutation<T>(IList<T> listA, IList<T> listB) {
            Stack<T> stackA = new Stack<T>(listA);

            int i = 0;
            while (stackA.Count > 0 && i < listB.Count) {
                T obj = stackA.Peek();
                for (i = 0; i < listB.Count; i++) {
                    if (listB[i].Equals(obj)) {
                        stackA.Pop();
                        break;
                    }
                }
            }

            return stackA.Count == 0;
        }

        public static bool EqualsPermutation<T>(T itemA1, T itemA2, T itemB1, T itemB2) {
            return (itemA1.Equals(itemB1) && itemA2.Equals(itemB2)) || (itemA1.Equals(itemB2) && itemA2.Equals(itemB1));
        }

        public static void Swap<T>(ref T itemA, ref T itemB) {
            T aux = itemA;
            itemA = itemB;
            itemB = aux;
        }

        public static bool InRange(float value, float min, float max) {
            return value >= min && value <= max;
        }

        public static bool InRangeExclusive(float value, float min, float max) {
            return value > min && value < max;
        }

        public static bool InRangeRightExclusive(float value, float min, float max) {
            return value >= min && value < max;
        }

        public static bool InRangeLeftExclusive(float value, float min, float max) {
            return value > min && value <= max;
        }

        #region IEnumerable

        public static IEnumerable<T> Iterate<T>(params IEnumerable<T>[] collections) {
            foreach (IEnumerable<T> collection in collections) {
                IEnumerator<T> enumerator = collection.GetEnumerator();
                while (enumerator.MoveNext()) {
                    yield return enumerator.Current;
                }
            }
        }

        #endregion IEnumerable

        #region TileMap

        public static Vector2 ConvertPositionToCell(Vector2 position, TileMap tilemap) {
            Rectangle tilemapBounds = new Rectangle(tilemap.Position - tilemap.Origin, tilemap.Size);
            position = Math.Clamp(position, tilemapBounds);
            return Math.Floor(position / tilemap.TileSize);
        }

        public static Vector2 ConvertCellToPosition(Vector2 cell, TileMap tilemap) {
            return (tilemap.Position - tilemap.Origin) + cell * tilemap.TileSize;
        }

        #endregion TileMap
    }
}
