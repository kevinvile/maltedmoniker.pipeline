using System.Reflection;
using System.Threading.Tasks;

namespace maltedmoniker.pipeline.Extensions
{
    internal static class MethodInfoExtensions
    {
        public static async Task<object> InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
        {
            dynamic awaitable = @this.Invoke(obj, BindingFlags.Public | BindingFlags.NonPublic, null, parameters, null);
            //var r = await awaitable;
            var task = (Task)awaitable;
            await task;
            return task.GetType().GetProperty("Result").GetValue(task);
            //return awaitable.GetAwaiter().GetResult();
        }
        
    }
}
