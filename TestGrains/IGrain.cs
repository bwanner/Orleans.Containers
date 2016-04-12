using System.Threading.Tasks;

namespace TestGrains
{
    public interface IGrain
    {
        Task Foo(TestObjectWithPropertyChange propertyChange);
    }
}