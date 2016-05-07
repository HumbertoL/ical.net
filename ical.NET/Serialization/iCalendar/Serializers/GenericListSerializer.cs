﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Interfaces.Serialization.Factory;

namespace Ical.Net.Serialization.iCalendar.Serializers
{
    public class GenericListSerializer : SerializerBase
    {
        #region Private Fields

        Type _innerType;
        Type _objectType;

        #endregion

        #region Constructors

        public GenericListSerializer(Type objectType)
        {
            _innerType = objectType.GetGenericArguments()[0];

            var listDef = typeof (List<>);
            _objectType = listDef.MakeGenericType(typeof (object));
        }

        #endregion

        #region Overrides

        public override Type TargetType => _objectType;

        public override string SerializeToString(object obj)
        {
            // NOTE: this behavior is implemented in the PropertySerializer.
            throw new NotImplementedException();
        }

        public override object Deserialize(TextReader tr)
        {
            var p = SerializationContext.Peek() as ICalendarProperty;
            if (p != null)
            {
                // Get a serializer factory to deserialize the contents of this list
                var sf = GetService<ISerializerFactory>();

                var listObj = Activator.CreateInstance(_objectType);
                if (listObj != null)
                {
                    // Get a serializer for the inner type
                    var stringSerializer = sf.Build(_innerType, SerializationContext) as IStringSerializer;
                    ;

                    if (stringSerializer != null)
                    {
                        // Deserialize the inner object
                        var value = tr.ReadToEnd();
                        var objToAdd = stringSerializer.Deserialize(new StringReader(value));

                        // If deserialization failed, pass the string value
                        // into the list.
                        if (objToAdd == null)
                        {
                            objToAdd = value;
                        }

                        if (objToAdd != null)
                        {
                            // FIXME: cache this
                            var mi = _objectType.GetMethod("Add");
                            if (mi != null)
                            {
                                // Determine if the returned object is an IList<ObjectType>,
                                // rather than just an ObjectType.
                                if (objToAdd is IEnumerable && objToAdd.GetType().Equals(typeof (List<>).MakeGenericType(_innerType)))
                                {
                                    // Deserialization returned an IList<ObjectType>, instead of
                                    // simply an ObjectType.  So, let's enumerate through the
                                    // items in the list and add them individually to our
                                    // list.
                                    foreach (var innerObj in (IEnumerable) objToAdd)
                                    {
                                        mi.Invoke(listObj, new[] {innerObj});
                                    }
                                }
                                else
                                {
                                    // Add the object to the list
                                    mi.Invoke(listObj, new[] {objToAdd});
                                }
                                return listObj;
                            }
                        }
                    }
                }
            }

            return null;
        }

        #endregion
    }
}