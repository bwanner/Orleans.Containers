using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Serialization;
using MetaLinq;

namespace Orleans.Streams
{
    /// <summary>
    ///     Serializable wrapper for an expression describing a Func&lt;TIn, TOut&gt;.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    [Serializable]
    public class SerializableFunc<TIn, TOut>
    {
        private readonly Type _type;

        private readonly string _xmlString;

        [NonSerialized] private Expression<Func<TIn, TOut>> _expression;

        /// <summary>
        /// Get the expression.
        /// </summary>
        public Expression<Func<TIn, TOut>> Value => ComputeExpression();

        public static implicit operator SerializableFunc<TIn, TOut>(Expression<Func<TIn, TOut>> expression)
        {
            return new SerializableFunc<TIn, TOut>(expression);
        }

        /// <summary>
        /// Create a new SerializableExpression.
        /// </summary>
        /// <param name="expression">Expression to serialize.</param>
        public SerializableFunc(Expression<Func<TIn, TOut>> expression)
        {
            var editableExpression = EditableExpression.CreateEditableExpression(expression);

            var ms = new MemoryStream();
            var serializer = new XmlSerializer(editableExpression.GetType());
            serializer.Serialize(ms, editableExpression);
            ms.Flush();
            _xmlString = Encoding.UTF8.GetString(ms.ToArray());
            _type = editableExpression.GetType();
        }

        private Expression<Func<TIn, TOut>> ComputeExpression()
        {
            if (_expression == null)
            {
                var serializer = new XmlSerializer(_type);
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(_xmlString));
                var o = serializer.Deserialize(ms);
                var editableExpression = (EditableExpression) o;
                _expression = (Expression<Func<TIn, TOut>>) editableExpression.ToExpression();
            }

            return _expression;
        }
    }
}