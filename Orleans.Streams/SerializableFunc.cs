using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Serialization;
using MetaLinq;

namespace Orleans.Streams
{
	
    /// <summary>
    ///     Serializable wrapper for an expression describing a Func from input types to an output type.
    /// </summary>
	/// <typeparam name="TOut"></typeparam>
    [Serializable]
    public class SerializableFunc<TOut>
    {
        private readonly Type _type;

        private readonly string _xmlString;

        [NonSerialized] private Expression<Func<TOut>> _expression;

        /// <summary>
        /// Get the expression.
        /// </summary>
        public Expression<Func<TOut>> Value => ComputeExpression();

        public static implicit operator SerializableFunc<TOut>(Expression<Func<TOut>> expression)
        {
            return new SerializableFunc<TOut>(expression);
        }

        /// <summary>
        /// Create a new SerializableExpression.
        /// </summary>
        /// <param name="expression">Expression to serialize.</param>
        public SerializableFunc(Expression<Func<TOut>> expression)
        {
            var editableExpression = EditableExpression.CreateEditableExpression(expression);

            var ms = new MemoryStream();
            var serializer = new XmlSerializer(editableExpression.GetType());
            serializer.Serialize(ms, editableExpression);
            ms.Flush();
            _xmlString = Encoding.UTF8.GetString(ms.ToArray());
            _type = editableExpression.GetType();
        }

        private Expression<Func<TOut>> ComputeExpression()
        {
            if (_expression == null)
            {
                var serializer = new XmlSerializer(_type);
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(_xmlString));
                var o = serializer.Deserialize(ms);
                var editableExpression = (EditableExpression) o;
                _expression = (Expression<Func<TOut>>) editableExpression.ToExpression();
            }

            return _expression;
        }
	}

    /// <summary>
    ///     Serializable wrapper for an expression describing a Func from input types to an output type.
    /// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="TOut"></typeparam>
    [Serializable]
    public class SerializableFunc<T1,TOut>
    {
        private readonly Type _type;

        private readonly string _xmlString;

        [NonSerialized] private Expression<Func<T1,TOut>> _expression;

        /// <summary>
        /// Get the expression.
        /// </summary>
        public Expression<Func<T1,TOut>> Value => ComputeExpression();

        public static implicit operator SerializableFunc<T1,TOut>(Expression<Func<T1,TOut>> expression)
        {
            return new SerializableFunc<T1,TOut>(expression);
        }

        /// <summary>
        /// Create a new SerializableExpression.
        /// </summary>
        /// <param name="expression">Expression to serialize.</param>
        public SerializableFunc(Expression<Func<T1,TOut>> expression)
        {
            var editableExpression = EditableExpression.CreateEditableExpression(expression);

            var ms = new MemoryStream();
            var serializer = new XmlSerializer(editableExpression.GetType());
            serializer.Serialize(ms, editableExpression);
            ms.Flush();
            _xmlString = Encoding.UTF8.GetString(ms.ToArray());
            _type = editableExpression.GetType();
        }

        private Expression<Func<T1,TOut>> ComputeExpression()
        {
            if (_expression == null)
            {
                var serializer = new XmlSerializer(_type);
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(_xmlString));
                var o = serializer.Deserialize(ms);
                var editableExpression = (EditableExpression) o;
                _expression = (Expression<Func<T1,TOut>>) editableExpression.ToExpression();
            }

            return _expression;
        }
	}

    /// <summary>
    ///     Serializable wrapper for an expression describing a Func from input types to an output type.
    /// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="T2"></typeparam>
	/// <typeparam name="TOut"></typeparam>
    [Serializable]
    public class SerializableFunc<T1, T2,TOut>
    {
        private readonly Type _type;

        private readonly string _xmlString;

        [NonSerialized] private Expression<Func<T1, T2,TOut>> _expression;

        /// <summary>
        /// Get the expression.
        /// </summary>
        public Expression<Func<T1, T2,TOut>> Value => ComputeExpression();

        public static implicit operator SerializableFunc<T1, T2,TOut>(Expression<Func<T1, T2,TOut>> expression)
        {
            return new SerializableFunc<T1, T2,TOut>(expression);
        }

        /// <summary>
        /// Create a new SerializableExpression.
        /// </summary>
        /// <param name="expression">Expression to serialize.</param>
        public SerializableFunc(Expression<Func<T1, T2,TOut>> expression)
        {
            var editableExpression = EditableExpression.CreateEditableExpression(expression);

            var ms = new MemoryStream();
            var serializer = new XmlSerializer(editableExpression.GetType());
            serializer.Serialize(ms, editableExpression);
            ms.Flush();
            _xmlString = Encoding.UTF8.GetString(ms.ToArray());
            _type = editableExpression.GetType();
        }

        private Expression<Func<T1, T2,TOut>> ComputeExpression()
        {
            if (_expression == null)
            {
                var serializer = new XmlSerializer(_type);
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(_xmlString));
                var o = serializer.Deserialize(ms);
                var editableExpression = (EditableExpression) o;
                _expression = (Expression<Func<T1, T2,TOut>>) editableExpression.ToExpression();
            }

            return _expression;
        }
	}
}