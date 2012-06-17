using System;
using System.Reflection;
using System.Linq.Expressions;

namespace MongoAccounting.Utils
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
    }
}
