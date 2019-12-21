using UnityEngine;

namespace GameObjects
{
    public class MaterialResize : MonoBehaviour
    {
        public enum Direction
        {
            XY, //0
            XZ, //1
            YX, //2
            YZ, //3
            ZX, //4
            ZY //5
        }
    
        public enum Anchor
        {
            NorthWest,
            North,
            NorthEast,
            West,
            Center,
            East,
            SouthWest,
            South,
            SouthEast
        }

        public Direction direction = Direction.XY;
        public Anchor anchor = Anchor.Center;

        private Vector2 _scaleFactor;
        private Vector2 _offset;
        private Vector2 _baseScale;

        private Material _mat;

        private void Start()
        {
            _mat = GetComponent<Renderer>().material;
            var scale = transform.localScale;
            _scaleFactor = _mat.mainTextureScale / GetScale(scale);
            _baseScale = _mat.mainTextureScale;
        }

        private Vector2 GetScale(Vector3 scale)
        {
            return new Vector2(GetScaleX(scale), GetScaleY(scale));
        }

        private float GetScaleX(Vector3 scale)
        {
            switch (direction)
            {
                case Direction.XY:
                case Direction.XZ:
                    return scale.x;
                case Direction.YX:
                case Direction.YZ:
                    return scale.y;
                case Direction.ZX:
                case Direction.ZY:
                    return scale.z;
                default:
                    return 0f;
            }
        }

        private float GetScaleY(Vector3 scale)
        {
            switch (direction)
            {
                case Direction.YX:
                case Direction.ZX:
                    return scale.x;
                case Direction.XY:
                case Direction.ZY:
                    return scale.y;
                case Direction.XZ:
                case Direction.YZ:
                    return scale.z;
                default:
                    return 0f;
            }
        }

        private Vector2 GetAnchor()
        {
            switch (anchor)
            {
                case Anchor.NorthWest:
                    return new Vector2(0,0);
                case Anchor.North:
                    return new Vector2(.5f,0);
                case Anchor.NorthEast:
                    return new Vector2(1,0);
                case Anchor.West:
                    return new Vector2(0,.5f);
                case Anchor.Center:
                    return new Vector2(.5f,.5f);
                case Anchor.East:
                    return new Vector2(1,.5f);
                case Anchor.SouthWest:
                    return new Vector2(0,1);
                case Anchor.South:
                    return new Vector2(.5f,1);
                case Anchor.SouthEast:
                    return new Vector2(1,1);
                default:
                    return new Vector2(0,0);
            }
        }

        private void Update()
        {
            var scale = transform.localScale;
            _mat.mainTextureScale = GetScale(scale) * _scaleFactor;
            _mat.mainTextureOffset = (_baseScale - _mat.mainTextureScale) * GetAnchor();
        }
    }
}