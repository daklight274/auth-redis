using Autofac;
using System.Reflection;
using Module = Autofac.Module;

namespace Auth_Redis.Autofac
{
    public class ApplicationModule: Module
    {
        private string _connectionString { get; }
        public ApplicationModule(string connectionString)
        {
            _connectionString = connectionString;
        }
        protected override void Load(ContainerBuilder builder)
        {

            // Lấy assembly hiện tại (có thể thay bằng assembly khác nếu cần)
            var assembly = Assembly.GetExecutingAssembly();

            // Đăng ký tất cả class kết thúc bằng "Repository"
            builder.RegisterAssemblyTypes(assembly)
                   .Where(t => t.Name.EndsWith("Repository"))
                   .AsImplementedInterfaces()      // tự động đăng ký theo interface
                   .InstancePerLifetimeScope()
                   .WithParameter("connectionString", _connectionString); ;    // lifecycle

            // Đăng ký tất cả class kết thúc bằng "Service"
            builder.RegisterAssemblyTypes(assembly)
                   .Where(t => t.Name.EndsWith("Service"))
                   .AsImplementedInterfaces()
                   .SingleInstance();
        }


    }

}
