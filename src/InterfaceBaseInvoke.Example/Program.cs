using System;

namespace InterfaceBaseInvoke.Example
{
    public interface IService
    {
        void Call()
        {
            Console.WriteLine("Call");
        }
    }

    public class Service : IService
    {
        public void Call()
        {
            Console.WriteLine("Before Call");
            this.Base<IService>().Call();
            Console.WriteLine("After Call");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var service = new Service();
            service.Call();
        }
    }
}
