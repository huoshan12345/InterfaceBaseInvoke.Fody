using System;

namespace InterfaceBaseInvoke.Example
{
    public interface IService
    {
        void Method();
    }

    public interface IService1 : IService
    {
        public const string Name = nameof(IService1) + "." + nameof(Method);
        void IService.Method()
        {
            Console.WriteLine(Name);
        }
    }

    public interface IService2 : IService
    {
        public const string Name = nameof(IService2) + "." + nameof(Method);
        void IService.Method()
        {
            Console.WriteLine(Name);
        }
    }

    public class Service : IService1, IService2
    {
        public void Method()
        {
            throw new InvalidOperationException();
        }

        public void Invoke()
        {
            Console.WriteLine("Start invoking...");
            this.Base<IService1>().Method();
            this.Base<IService2>().Method();
            Console.WriteLine("End invoking.");
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
