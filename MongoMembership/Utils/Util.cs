using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Configuration;

namespace MongoMembership.Utils
{
    internal static class Util
    {
        public static T GetValue<T>(string value, T defaultValue)
        {
            if (value.IsNullOrEmpty())
                return defaultValue;

            return ((T)Convert.ChangeType(value, typeof(T)));
        }

        public static string GetElementNameFor<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var type = typeof(TSource);

            var memberExpression = propertyLambda.Body as MemberExpression;
            if (memberExpression == null && propertyLambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)propertyLambda.Body;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            memberExpression.EnsureNotNull("memberExpression");

            var propInfo = memberExpression.Member as PropertyInfo;
            propInfo.EnsureNotNull("propInfo");

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException("Expression '{0}' refers to a property that is not from type {1}.".F(propertyLambda, type));

            return propInfo.Name;
        }

        public static string GetElementNameFor<TSource>(Expression<Func<TSource, object>> propertyLambda)
        {
            return GetElementNameFor<TSource, object>(propertyLambda);
        }
        
        public static string GetConnectionStringByName(string connectionsettingskeys)
        {
            var keys = connectionsettingskeys.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var key in keys)
            {
                var name = key.Trim(new[] { ' ' });

                if (name.IsNullOrEmpty())
                    continue;

                var connectionString1 = ConfigurationManager.ConnectionStrings[name];
                if (connectionString1 != null)
                    return connectionString1.ConnectionString;

                var connectionString2 = ConfigurationManager.AppSettings[name];
                if (connectionString2 != null)
                    return connectionString2;
            }
            return "mongodb://localhost/MongoMembership";
        }
    }
}
