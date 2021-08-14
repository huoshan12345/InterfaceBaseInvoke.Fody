using System;

namespace InterfaceBaseInvoke.Example
{
    public interface IService
    {
        public const string Name = nameof(IService) + "." + nameof(Method);
        void Method()
        {
            Console.WriteLine(Name);
        }
    }

    public interface IService2
    {
        void Method()
        {
            Console.WriteLine(nameof(IService) + "." + nameof(Method));
        }
    }

    public class Service : IService, IService2
    {
        public void Method()
        {
            throw new InvalidOperationException();
        }

        public void Invoke()
        {
            Console.WriteLine("Before " + IService.Name);
            this.Base<IService>().Method();
            Console.WriteLine("After " + IService.Name);
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = new Service();
            service.Invoke();
        }
    }
}
