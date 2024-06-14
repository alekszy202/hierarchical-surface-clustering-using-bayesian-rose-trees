namespace Interfaces
{
    public interface IVector
    {
        IVector Subtract(IVector subtrahend);
        double Dot(IVector other);
        IMatrix OuterProduct();
        bool Equals(IVector other);
    }
}
