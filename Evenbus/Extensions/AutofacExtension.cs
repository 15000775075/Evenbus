using Autofac;
using Microsoft.AspNetCore.Mvc;

namespace Evenbus.Extensions
{
    public class AutofacExtension : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            System.Type controllerBaseType = typeof(ControllerBase);
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .Where(t => controllerBaseType.IsAssignableFrom(t) && t != controllerBaseType)
                .PropertiesAutowired();
        }
    }
}