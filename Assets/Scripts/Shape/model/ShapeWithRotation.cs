namespace Shape.model
{
    public class ShapeWithRotation
    {
        public ShapeWithRotation(ShapeSO shape, int rotation)
        {
            Shape = shape;
            Rotation = rotation;
        }

        public int Rotation { get; private set; }

        public ShapeSO Shape { get; private set; }

        public void Rotate(int dir)
        {
            var rotation = Rotation + dir;
            if (rotation < 0)
            {
                rotation += 4;
            }

            if (rotation > 3)
            {
                rotation -= 4;
            }

            Rotation = rotation;
        }
    }
}