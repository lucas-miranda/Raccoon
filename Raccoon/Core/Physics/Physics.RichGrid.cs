using Raccoon.Components;

namespace Raccoon {
    public partial class Physics {
        #region RichGrid vs RichGrid

        private bool CheckRichGridRichGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #endregion RichGrid vs RichGrid

        #region RichGrid vs Polygon

        private bool CheckRichGridPolygon(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckPolygonRichGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion RichGrid vs Polygon

        #region RichGrid vs Box

        private bool CheckRichGridBox(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckBoxRichGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion RichGrid vs Box

        #region RichGrid vs Grid

        private bool CheckRichGridGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckGridRichGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion RichGrid vs Grid

        #region RichGrid vs Circle

        private bool CheckRichGridCircle(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckCircleRichGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion RichGrid vs Circle

        #region RichGrid vs Line

        private bool CheckRichGridLine(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckLineRichGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion RichGrid vs Line
    }
}
