using Raccoon.Components;

namespace Raccoon {
    public partial class Physics {
        #region Grid vs Grid

        private bool CheckGridGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #endregion Grid vs Grid

        #region Grid vs Box

        private bool CheckGridBox(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckBoxGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Grid vs Box

        #region Grid vs Circle

        private bool CheckGridCircle(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckCircleGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Grid vs Circle

        #region Grid vs Line

        private bool CheckGridLine(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckLineGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Grid vs Line

        #region Grid vs Polygon

        private bool CheckGridPolygon(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckPolygonGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Grid vs Polygon

        #region Grid vs RichGrid

        private bool CheckGridRichGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return true;
        }

        #endregion Grid vs RichGrid
    }
}
