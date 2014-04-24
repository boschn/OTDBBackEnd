
REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** ORM DATA STRUCTURE CLASSES
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2014-04-24
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************''' <summary>

Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic

Imports System.Reflection


Namespace OnTrack.Database

    ''' <summary>
    ''' OrdinalType identifies the data type of the ordinal
    ''' </summary>
    ''' <remarks></remarks>

    Public Enum OrdinalType
        longType
        stringType
    End Enum
    ''' <summary>
    ''' ordinal class describes values as ordinal values (ordering)
    ''' </summary>
    ''' <remarks></remarks>

    Public Class Ordinal
        Implements IEqualityComparer(Of Ordinal)
        Implements IConvertible
        Implements IComparable(Of Ordinal)
        Implements IComparer(Of Ordinal)

        Private _ordinalvalue As Object
        Private _ordinalType As OrdinalType

        Public Sub New(ByVal value As Object)
            ' return depending on the type

            If TypeOf value Is Long Or TypeOf value Is Integer Or TypeOf value Is UShort _
            Or TypeOf value Is Short Or TypeOf value Is UInteger Or TypeOf value Is ULong Then
                _ordinalType = OrdinalType.longType
                _ordinalvalue = CLng(value)
            ElseIf IsNumeric(value) Then
                _ordinalType = OrdinalType.longType
                _ordinalvalue = CLng(value)
            ElseIf TypeOf value Is Ordinal Then
                _ordinalType = CType(value, Ordinal).Type
                _ordinalvalue = CType(value, Ordinal).Value

            ElseIf value IsNot Nothing AndAlso value.ToString Then
                _ordinalType = OrdinalType.stringType
                _ordinalvalue = String.Copy(value.ToString)
            Else
                Throw New Exception("value is not casteable to a XMAPordinalType")

            End If

        End Sub
        Public Sub New(ByVal value As Object, ByVal type As OrdinalType)
            _ordinalType = type
            Me.Value = value
        End Sub
        Public Sub New(ByVal type As OrdinalType)
            _ordinalType = type
            _ordinalvalue = Nothing
        End Sub

        Public Function ToString() As String
            Return _ordinalvalue.ToString
        End Function
        ''' <summary>
        ''' Equalses the specified x.
        ''' </summary>
        ''' <param name="x">The x.</param>
        ''' <param name="y">The y.</param>
        ''' <returns></returns>
        Public Function [Equals](x As Ordinal, y As Ordinal) As Boolean Implements IEqualityComparer(Of Ordinal).[Equals]
            Select Case x._ordinalType
                Case OrdinalType.longType
                    Return x.Value.Equals(y.Value)
                Case OrdinalType.stringType
                    If String.Compare(x.Value, y.Value, False) = 0 Then
                        Return True
                    Else
                        Return False
                    End If
            End Select

            Return x.Value = y.Value
        End Function
        ''' <summary>
        ''' Compares two objects and returns a value indicating whether one is less
        ''' than, equal to, or greater than the other.
        ''' </summary>
        ''' <param name="x">The first object to compare.</param>
        ''' <param name="y">The second object to compare.</param>
        ''' <exception cref="T:System.ArgumentException">Neither <paramref name="x" /> nor
        ''' <paramref name="y" /> implements the <see cref="T:System.IComparable" /> interface.-or-
        ''' <paramref name="x" /> and <paramref name="y" /> are of different types and neither
        ''' one can handle comparisons with the other. </exception>
        ''' <returns>
        ''' A signed integer that indicates the relative values of <paramref name="x" />
        ''' and <paramref name="y" />, as shown in the following table.Value Meaning Less
        ''' than zero <paramref name="x" /> is less than <paramref name="y" />. Zero <paramref name="x" />
        ''' equals <paramref name="y" />. Greater than zero <paramref name="x" /> is greater
        ''' than <paramref name="y" />.
        ''' </returns>
        Public Function [Compare](x As Ordinal, y As Ordinal) As Integer Implements IComparer(Of Ordinal).[Compare]

            '** depend on the type
            Select Case x.Type
                Case OrdinalType.longType
                    ' try to compare numeric
                    If IsNumeric(y.Value) Then
                        If Me.Value > CLng(y.Value) Then
                            Return 1
                        ElseIf Me.Value < CLng(y.Value) Then
                            Return -1
                        Else
                            Return 0

                        End If
                    Else
                        Return -1
                    End If
                Case OrdinalType.stringType
                    Return String.Compare(y.Value, y.Value.ToString)

            End Select
        End Function
        ''' <summary>
        ''' Compares to.
        ''' </summary>
        ''' <param name="other">The other.</param>
        ''' <returns></returns>
        Public Function CompareTo(other As Ordinal) As Integer Implements IComparable(Of Ordinal).CompareTo
            Return Compare(Me, other)

        End Function

        ''' <summary>
        ''' Gets the hash code.
        ''' </summary>
        ''' <param name="obj">The obj.</param>
        ''' <returns></returns>
        Public Function GetHashCode(obj As Ordinal) As Integer Implements IEqualityComparer(Of Ordinal).GetHashCode
            Return _ordinalvalue.GetHashCode
        End Function
        ''' <summary>
        ''' Value of the ordinal
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Value As Object
            Get
                Select Case Me.Type
                    Case OrdinalType.longType
                        Return CLng(_ordinalvalue)
                    Case OrdinalType.stringType
                        Return CStr(_ordinalvalue)
                End Select
                Return Nothing
            End Get
            Set(value As Object)
                Select Case Me.Type
                    Case OrdinalType.longType
                        _ordinalvalue = CLng(value)
                    Case OrdinalType.stringType
                        _ordinalvalue = CStr(value)
                End Select

                _ordinalvalue = value
            End Set

        End Property
        ''' <summary>
        ''' Datatype of the ordinal
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Type As OrdinalType
            Get
                Return _ordinalType
            End Get
        End Property
        ''' <summary>
        ''' gets the Typecode of the ordinal
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetTypeCode() As TypeCode Implements IConvertible.GetTypeCode
            If _ordinalType = OrdinalType.longType Then
                Return TypeCode.UInt64
            ElseIf _ordinalType = OrdinalType.stringType Then
                Return TypeCode.String
            Else
                Return TypeCode.Object
            End If

        End Function

        Public Function ToBoolean(provider As IFormatProvider) As Boolean Implements IConvertible.ToBoolean
            Return _ordinalvalue <> Nothing
        End Function

        Public Function ToByte(provider As IFormatProvider) As Byte Implements IConvertible.ToByte
            Return Convert.ToByte(_ordinalvalue)
        End Function

        Public Function ToChar(provider As IFormatProvider) As Char Implements IConvertible.ToChar
            Return Convert.ToChar(_ordinalvalue)
        End Function

        Public Function ToDateTime(provider As IFormatProvider) As Date Implements IConvertible.ToDateTime

        End Function

        Public Function ToDecimal(provider As IFormatProvider) As Decimal Implements IConvertible.ToDecimal
            Return Convert.ToDecimal(_ordinalvalue)
        End Function

        Public Function ToDouble(provider As IFormatProvider) As Double Implements IConvertible.ToDouble
            Return Convert.ToDouble(_ordinalvalue)
        End Function

        Public Function ToInt16(provider As IFormatProvider) As Short Implements IConvertible.ToInt16
            Return Convert.ToInt16(_ordinalvalue)
        End Function

        Public Function ToInt32(provider As IFormatProvider) As Integer Implements IConvertible.ToInt32
            Return Convert.ToInt32(_ordinalvalue)
        End Function

        Public Function ToInt64(provider As IFormatProvider) As Long Implements IConvertible.ToInt64
            Return Convert.ToInt64(_ordinalvalue)
        End Function

        Public Function ToSByte(provider As IFormatProvider) As SByte Implements IConvertible.ToSByte
            Return Convert.ToSByte(_ordinalvalue)
        End Function

        Public Function ToSingle(provider As IFormatProvider) As Single Implements IConvertible.ToSingle
            Return Convert.ToSingle(_ordinalvalue)
        End Function

        Public Function ToString(provider As IFormatProvider) As String Implements IConvertible.ToString
            Return Convert.ToString(_ordinalvalue)
        End Function

        Public Function ToType(conversionType As Type, provider As IFormatProvider) As Object Implements IConvertible.ToType
            ' DirectCast(_ordinalvalue, conversionType)
        End Function

        Public Function ToUInt16(provider As IFormatProvider) As UShort Implements IConvertible.ToUInt16
            Return Convert.ToUInt16(_ordinalvalue)
        End Function

        Public Function ToUInt32(provider As IFormatProvider) As UInteger Implements IConvertible.ToUInt32
            Return Convert.ToUInt32(_ordinalvalue)
        End Function

        Public Function ToUInt64(provider As IFormatProvider) As ULong Implements IConvertible.ToUInt64
            Return Convert.ToUInt64(_ordinalvalue)
        End Function

        Public Shared Operator =(x As Ordinal, y As Ordinal) As Boolean
            Return x.Value = y.Value
        End Operator
        Public Shared Operator <(x As Ordinal, y As Ordinal) As Boolean
            Return x.Value < y.Value
        End Operator
        Public Shared Operator >(x As Ordinal, y As Ordinal) As Boolean
            Return x.Value > y.Value
        End Operator
        Public Shared Operator <>(x As Ordinal, y As Ordinal) As Boolean
            Return x.Value <> y.Value
        End Operator
        Public Shared Operator +(x As Ordinal, y As Ordinal) As Boolean
            Return x.Value + y.Value
        End Operator

        Function ToUInt64() As Integer
            If IsNumeric(_ordinalvalue) Then Return CLng(_ordinalvalue)
            Throw New NotImplementedException
        End Function
        ''' <summary>
        ''' compares this to an ordinal
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Equals(value As Ordinal) As Boolean
            Return Me.Compare(Me, value) = 0
        End Function

    End Class

    ''' <summary>
    ''' Enumerator for QueryEnumeration
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ormRelationCollectionEnumerator(Of T As {iormInfusable, iormPersistable})
        Implements IEnumerator

        Private _collection As ormRelationCollection(Of T)
        Private _counter As Integer
        Private _keyvalues As IList
        Public Sub New(collection As ormRelationCollection(Of T))
            _collection = collection
            _keyvalues = _collection.Keys
            _counter = -1
        End Sub
        Public ReadOnly Property Current As Object Implements IEnumerator.Current
            Get
                If _counter >= 0 And _counter < _keyvalues.Count Then Return _collection.Item(key:=_keyvalues.Item(_counter))
                ' throw else
                Throw New InvalidOperationException()
            End Get
        End Property

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            _counter += 1
            Return (_counter < _keyvalues.Count)
            ' throw else
            Throw New InvalidOperationException()
        End Function

        Public Sub Reset() Implements IEnumerator.Reset
            _counter = 0
        End Sub
    End Class

    ''' <summary>
    '''  Interface
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>
    Public Interface iormRelationalCollection(Of T)
        Inherits ICollection(Of T)

        Property Item(key As Object) As T
        Property Item(keys As Object()) As T

        Property item(key As DataKeyTuple) As T


        Function ContainsKey(keys As Object()) As Boolean
        Function ContainsKey(key As Object) As Boolean

        Function ContainsKey(key As DataKeyTuple) As Boolean


        Function GetKeyValues(item As T) As DataKeyTuple

        Function GetKeyNames() As String()
    End Interface

    ''' <summary>
    ''' describes an RelationCollection which can add new iormpersistables by key
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>

    Public Class ormRelationNewableCollection(Of T As {New, iormInfusable, iormPersistable})
        Inherits ormRelationCollection(Of T)

        Public Event OnNew(sender As Object, e As ormRelationCollection(Of T).EventArgs)

        ''' <summary>
        ''' constructor with the container object (of iormpersistable) 
        ''' and keyentrynames of T
        ''' </summary>
        ''' <param name="containerobject"></param>
        ''' <param name="keynames"></param>
        ''' <remarks></remarks>
        Public Sub New(container As iormPersistable, keyentrynames As String())
            MyBase.New(container:=container, keyentrynames:=keyentrynames)
        End Sub

        ''' <summary>
        ''' create a new item already stored in this collection
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddNew(keys As Object()) As T
            Dim anItem As T = Activator.CreateInstance(Of T)()


            ' set the values in the object
            For i = 0 To _keyentries.Count - 1
                keys(i) = anItem.SetValue(_keyentries(i), keys(i))
            Next i

            Dim args = New ormRelationCollection(Of T).EventArgs(anItem)
            RaiseEvent OnNew(Me, args)
            If args.Cancel Then Return Nothing

            Dim arecord As New ormRecord
            If args.Dataobject.Feed(arecord) Then
                anItem = ormDataObject.CreateDataObject(Of T)(arecord)
                If anItem IsNot Nothing Then
                    Me.Add(anItem)
                    Return anItem
                End If
            End If
            Return Nothing
        End Function
    End Class

    ''' <summary>
    ''' Implementation of an Relational Collection
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>

    Public Class ormRelationCollection(Of T As {iormInfusable, iormPersistable})
        Implements iormRelationalCollection(Of T)

        Public Class EventArgs
            Inherits CancelEventArgs

            Private _dataobject As T

            Public Sub New(ByRef dataobject As T)
                _dataobject = dataobject
            End Sub


            ''' <summary>
            ''' Gets or sets the dataobject.
            ''' </summary>
            ''' <value>The dataobject.</value>
            Public Property Dataobject() As T
                Get
                    Return Me._dataobject
                End Get
                Set(value As T)
                    Me._dataobject = value
                End Set
            End Property

        End Class

        Private _dictionary As New SortedDictionary(Of DataKeyTuple, iormPersistable)
        Private _container As iormPersistable

        Protected _keyentries As String()

        Public Event OnAdding(sender As Object, e As ormRelationCollection(Of T).EventArgs)
        Public Event OnAdded(sender As Object, e As ormRelationCollection(Of T).EventArgs)

        Public Event OnRemoving(sender As Object, e As ormRelationCollection(Of T).EventArgs)
        Public Event OnRemoved(sender As Object, e As ormRelationCollection(Of T).EventArgs)

        ''' <summary>
        ''' constructor with the container object (of iormpersistable) 
        ''' and keyentrynames of T
        ''' </summary>
        ''' <param name="containerobject"></param>
        ''' <param name="keynames"></param>
        ''' <remarks></remarks>
        Public Sub New(container As iormPersistable, keyentrynames As String())
            _container = container
            _keyentries = keyentrynames
        End Sub
        ''' <summary>
        ''' get the size
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Size As UShort
            Get
                Return _keyentries.GetUpperBound(0) + 1
            End Get
        End Property
        ''' <summary>
        ''' gets the list of keys in the collection
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Keys As IList(Of DataKeyTuple)
            Get
                Return _dictionary.Keys.ToList
            End Get

        End Property
        ''' <summary>
        ''' returns the entry names for the keys in the collection
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetKeyNames() As String() Implements iormRelationalCollection(Of T).GetKeyNames
            Return _keyentries
        End Function

        ''' <summary>
        ''' extract the key values of the item (keyentries)
        ''' </summary>
        ''' <param name="item"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetKeyValues(item As T) As DataKeyTuple Implements iormRelationalCollection(Of T).getKeyvalues
            Dim keys As New DataKeyTuple(Me.Size)
            For i = 0 To _keyentries.Count - 1
                keys.Item(i) = item.GetValue(_keyentries(i))
            Next i
            Return keys
        End Function

        ''' <summary>
        ''' add an item to the collection - notifies container
        ''' </summary>
        ''' <param name="item"></param>
        ''' <remarks></remarks>
        Public Sub Add(item As T) Implements ICollection(Of T).Add
            Dim args = New ormRelationCollection(Of T).EventArgs(item)
            RaiseEvent OnAdding(Me, args)
            If args.Cancel Then Return

            ''' get the keys
            Dim keys = GetKeyValues(item)

            '' no error if we are already in this collection
            If Not Me.ContainsKey(keys) Then
                ''' add the handler for the delete event
                AddHandler item.OnDeleting, AddressOf IormPersistable_OnDelete
                ''' add to the dictionary
                _dictionary.Add(key:=keys, value:=item)
                ''' raise the event
                RaiseEvent OnAdded(Me, args)
            End If

        End Sub

        ''' <summary>
        ''' handler for the OnDeleting Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub IormPersistable_OnDelete(sender As Object, e As ormDataObjectEventArgs)
            Dim anItem As iormPersistable = e.DataObject
            Me.Remove(anItem)
        End Sub
        ''' <summary>
        ''' clear the Collection - is not a remove with handler
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Clear() Implements ICollection(Of T).Clear
            _dictionary.Clear()
        End Sub
        ''' <summary>
        ''' returns true if the key is in the collection
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ContainsKey(keys As Object()) As Boolean Implements iormRelationalCollection(Of T).containsKey
            Dim aKey As New DataKeyTuple(keys.GetUpperBound(0) + 1)
            aKey.Values = keys
            Return _dictionary.ContainsKey(key:=aKey)
        End Function
        ''' <summary>
        ''' returns true if the key is in the collection
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ContainsKey(keys As DataKeyTuple) As Boolean Implements iormRelationalCollection(Of T).containsKey
            Return _dictionary.ContainsKey(key:=keys)
        End Function

        ''' <summary>
        ''' returns true if the key is in the collection
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ContainsKey(key As Object) As Boolean Implements iormRelationalCollection(Of T).ContainsKey
            Dim aKey As New DataKeyTuple(1)
            aKey.Values = {key}
            Return _dictionary.ContainsKey(key:=aKey)
        End Function
        ''' <summary>
        ''' returns true if the item is in the collection. based on same keys
        ''' </summary>
        ''' <param name="item"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Contains(item As T) As Boolean Implements ICollection(Of T).Contains
            Dim keys = GetKeyValues(item)
            Return ContainsKey(keys)
        End Function
        ''' <summary>
        ''' copy out to an array
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="arrayIndex"></param>
        ''' <remarks></remarks>
        Public Sub CopyTo(array() As T, arrayIndex As Integer) Implements ICollection(Of T).CopyTo
            Dim anArray = _dictionary.Values.ToArray
            For i = arrayIndex To anArray.GetUpperBound(0)
                array(i) = anArray(i)
            Next

        End Sub
        ''' <summary>
        ''' count the number of items in the collection
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Count As Integer Implements ICollection(Of T).Count
            Get
                Return _dictionary.Count
            End Get
        End Property
        ''' <summary>
        ''' return true if readonly
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of T).IsReadOnly
            Get
                Return False
            End Get
        End Property
        ''' <summary>
        ''' remove an item from the collection - the delete handler of the container will be called 
        ''' which might lead to an delete of the item itself
        ''' </summary>
        ''' <param name="item"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Remove(item As T) As Boolean Implements ICollection(Of T).Remove
            Dim args = New ormRelationCollection(Of T).EventArgs(item)
            RaiseEvent OnRemoving(Me, args)

            Dim keys = GetKeyValues(item)
            Dim result = _dictionary.Remove(key:=keys)

            RaiseEvent OnRemoved(Me, args)
            Return result
        End Function
        ''' <summary>
        ''' gets an item by keys
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Item(keys As Object()) As T Implements iormRelationalCollection(Of T).item
            Get
                Dim aKey As New DataKeyTuple(keys.GetUpperBound(0) + 1)
                aKey.Values = keys
                Return Me.Item(aKey)
            End Get
            Set(value As T)
                Dim aKey As New DataKeyTuple(keys.GetUpperBound(0) + 1)
                aKey.Values = keys
                Me.Item(aKey) = value
            End Set
        End Property
        ''' <summary>
        ''' gets an item by keys
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Item(key As DataKeyTuple) As T Implements iormRelationalCollection(Of T).item
            Get
                If ContainsKey(key) Then Return _dictionary.Item(key:=key)
            End Get
            Set(value As T)
                If Not ContainsKey(key) Then _dictionary.Add(key:=key, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' gets an item by keys
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Item(key As Object) As T Implements iormRelationalCollection(Of T).item
            Get
                ' strange we cannot overload
                If key.GetType.Equals(GetType(DataKeyTuple)) Then
                    Return _dictionary.Item(key:=key)
                Else
                    Dim aKey As New DataKeyTuple(1)
                    aKey.Values = {key}
                    Return Me.Item(aKey)
                End If

            End Get
            Set(value As T)
                ' strange we cannot overload
                If key.GetType.Equals(GetType(DataKeyTuple)) Then
                    _dictionary.Add(key:=key, value:=value)
                Else
                    Dim aKey As New DataKeyTuple(1)
                    aKey.Values = {key}
                    Me.Item(aKey) = value
                End If

            End Set
        End Property
        ''' <summary>
        ''' returns an enumerator
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Return New ormRelationCollectionEnumerator(Of T)(Me)
        End Function

        Public Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return New ormRelationCollectionEnumerator(Of T)(Me)
        End Function
    End Class



    ''' <summary>
    ''' class for a Property Store with weighted properties for multiple property sets
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ComplexPropertyStore


        ''' <summary>
        ''' Event Arguments
        ''' </summary>
        ''' <remarks></remarks>
        Public Class EventArgs
            Inherits System.EventArgs

            Private _propertyname As String
            Private _setname As String
            Private _weight As Nullable(Of UShort)
            Private _value As Object

            Sub New(Optional propertyname As String = Nothing, Optional setname As String = Nothing, Optional weight As Nullable(Of UShort) = Nothing, Optional value As Object = Nothing)
                If propertyname IsNot Nothing Then _propertyname = propertyname
                If setname IsNot Nothing Then _setname = setname
                If weight.HasValue Then _weight = weight
                If value IsNot Nothing Then value = _value
            End Sub


            ''' <summary>
            ''' Gets the value.
            ''' </summary>
            ''' <value>The value.</value>
            Public ReadOnly Property Value() As Object
                Get
                    Return Me._value
                End Get
            End Property

            ''' <summary>
            ''' Gets the weight.
            ''' </summary>
            ''' <value>The weight.</value>
            Public ReadOnly Property Weight() As UShort?
                Get
                    Return Me._weight
                End Get
            End Property

            ''' <summary>
            ''' Gets the setname.
            ''' </summary>
            ''' <value>The setname.</value>
            Public ReadOnly Property Setname() As String
                Get
                    Return Me._setname
                End Get
            End Property

            ''' <summary>
            ''' Gets the propertyname.
            ''' </summary>
            ''' <value>The propertyname.</value>
            Public ReadOnly Property Propertyname() As String
                Get
                    Return Me._propertyname
                End Get
            End Property

        End Class

        ''' <summary>
        '''  Sequenze of sets
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum Sequence
            Primary = 0
            Secondary = 1
        End Enum

        ''' <summary>
        ''' main data structure a set by name consists of different properties with weights for the values
        ''' </summary>
        ''' <remarks></remarks>
        Private _sets As New Dictionary(Of String, Dictionary(Of String, SortedList(Of UShort, Object)))

        Private _currentset As String
        Private _defaultset As String = ""

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="defaultsetname"></param>
        ''' <remarks></remarks>
        Sub New(defaultsetname As String)
            _defaultset = defaultsetname
        End Sub
        ''' <summary>
        ''' Gets or sets the currentset.
        ''' </summary>
        ''' <value>The currentset.</value>
        Public Property CurrentSet() As String
            Get
                Return Me._currentset
            End Get
            Set(value As String)
                If Me.HasSet(value) Then
                    Me._currentset = value
                    RaiseEvent OnCurrentSetChanged(Me, New ComplexPropertyStore.EventArgs(setname:=value))
                Else
                    Throw New IndexOutOfRangeException(message:="set name '" & value & "' does not exist in the store")
                End If

            End Set
        End Property
        ''' <summary>
        ''' Event OnPropertyChange
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Event OnPropertyChanged(sender As Object, e As ComplexPropertyStore.EventArgs)
        Public Event OnCurrentSetChanged(sender As Object, e As ComplexPropertyStore.EventArgs)
        ''' <summary>
        ''' returns the config set for a setname with a driversequence
        ''' </summary>
        ''' <param name="setname"></param>
        ''' <param name="driverseq"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSet(setname As String, Optional sequence As Sequence = Sequence.Primary) As Dictionary(Of String, SortedList(Of UShort, Object))
            If HasConfigSetName(setname, sequence) Then
                Return _sets.Item(key:=setname & ":" & sequence)
            End If
        End Function
        ''' <summary>
        ''' returns the config set for a setname with a driversequence
        ''' </summary>
        ''' <param name="setname"></param>
        ''' <param name="driverseq"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasProperty(name As String, Optional setname As String = Nothing, Optional sequence As Sequence = Sequence.Primary) As Boolean
            If setname Is Nothing Then
                setname = _currentset
            End If
            If setname Is Nothing Then
                setname = _defaultset
            End If
            If HasSet(setname, sequence) Then
                Dim aset = GetSet(setname:=setname, sequence:=sequence)
                Return aset.ContainsKey(key:=name)
            End If
            Return False
        End Function

        ''' <summary>
        ''' sets a Property to the TableStore
        ''' </summary>
        ''' <param name="Name">Name of the Property</param>
        ''' <param name="Object">ObjectValue</param>
        ''' <returns>returns True if succesfull</returns>
        ''' <remarks></remarks>
        Public Function SetProperty(ByVal name As String, ByVal value As Object, _
                                    Optional ByVal weight As UShort = 0,
                                    Optional setname As String = "", _
                                    Optional sequence As Sequence = Sequence.Primary) As Boolean

            Dim aWeightedList As SortedList(Of UShort, Object)
            Dim aSet As Dictionary(Of String, SortedList(Of UShort, Object))
            If setname = "" Then
                setname = _defaultset
            End If

            If HasConfigSetName(setname, sequence) Then
                aSet = GetSet(setname, sequence:=sequence)
            Else
                aSet = New Dictionary(Of String, SortedList(Of UShort, Object))
                _sets.Add(key:=setname & ":" & sequence, value:=aSet)
            End If

            If aSet.ContainsKey(name) Then
                aWeightedList = aSet.Item(name)
                ' weight missing
                If weight = 0 Then
                    weight = aWeightedList.Keys.Max + 1
                End If
                ' retrieve
                If aWeightedList.ContainsKey(weight) Then
                    aWeightedList.Remove(weight)

                End If
                aWeightedList.Add(weight, value)
            Else
                aWeightedList = New SortedList(Of UShort, Object)
                '* get weight
                If weight = 0 Then
                    weight = 1
                End If
                aWeightedList.Add(weight, value)
                aSet.Add(name, aWeightedList)
            End If

            RaiseEvent OnPropertyChanged(Me, New ComplexPropertyStore.EventArgs(propertyname:=name, setname:=setname, weight:=weight, value:=value))
            Return True
        End Function
        ''' <summary>
        ''' Gets the Property of a config set. if setname is ommitted then check currentconfigset and the global one
        ''' </summary>
        ''' <param name="name">name of property</param>
        ''' <returns>object of the property</returns>
        ''' <remarks></remarks>
        Public Function GetProperty(ByVal name As String, Optional weight As UShort = 0, _
        Optional setname As String = "", _
        Optional sequence As Sequence = Sequence.Primary) As Object

            Dim aConfigSet As Dictionary(Of String, SortedList(Of UShort, Object))
            If setname = "" Then
                setname = _currentset
            End If
            '* test
            If setname <> "" AndAlso HasProperty(name, setname:=setname, sequence:=sequence) Then
                aConfigSet = GetSet(setname, sequence)
            ElseIf setname <> "" AndAlso HasProperty(name, setname:=setname) Then
                aConfigSet = GetSet(setname)
            ElseIf setname = "" AndAlso _currentset IsNot Nothing AndAlso HasProperty(name, setname:=_currentset, sequence:=sequence) Then
                setname = _currentset
                aConfigSet = GetSet(setname, sequence)
            ElseIf setname = "" AndAlso _defaultset IsNot Nothing AndAlso HasProperty(name, setname:=_defaultset) Then
                setname = _defaultset
                aConfigSet = GetSet(setname)
            Else
                Return Nothing
            End If
            ' retrieve
            Dim aWeightedList As SortedList(Of UShort, Object)
            If aConfigSet.ContainsKey(name) Then
                aWeightedList = aConfigSet.Item(name)
                If aWeightedList.ContainsKey(weight) Then
                    Return aWeightedList.Item(weight)
                ElseIf weight = 0 Then
                    Return aWeightedList.Last.Value
                Else
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns a list of selectable config set names without global
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ConfigSetNamesToSelect As List(Of String)
            Get
                Return ot.ConfigSetNames.FindAll(Function(x) x <> ConstGlobalConfigSetName)
            End Get
        End Property
        ''' <summary>
        ''' returns a list of ConfigSetnames
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property SetNames As List(Of String)
            Get
                Dim aList As New List(Of String)

                For Each name In _sets.Keys
                    If name.Contains(":") Then
                        name = name.Substring(0, name.IndexOf(":"))
                    End If
                    If Not aList.Contains(name) Then aList.Add(name)
                Next
                Return aList
            End Get
        End Property

        ''' <summary>
        ''' returns true if the config-set name exists 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasSet(ByVal setname As String, Optional sequence As Sequence = Sequence.Primary) As Boolean
            If _sets.ContainsKey(setname & ":" & sequence) Then
                Return True
            Else
                Return False
            End If
        End Function

    End Class

    ''' <summary>
    ''' Registery with some meta information
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ormDataTupleMetaClass(Of T)
        Private _objecttype As System.Type
        Private _objecttypename As String = ""
        Private _noKeys As UShort

        Private _lockobject As New Object

        ''' <summary>
        ''' constructor with an ormDataObject Class Type
        ''' </summary>
        ''' <param name="type"></param>
        ''' <remarks></remarks>
        Public Sub New([type] As System.Type)
            If [type].GetInterfaces.Contains(GetType(T)) OrElse [type].IsAssignableFrom(GetType(T)) Then
                Dim aDescriptor = ot.GetObjectClassDescription([type])
                If aDescriptor IsNot Nothing Then
                    _noKeys = aDescriptor.PrimaryKeyEntryNames.Count
                Else
                    Throw New Exception("registerentry: descriptor not found")
                End If
            Else
                Throw New Exception("registeryEntry: " & [type].Name & " has no interface or base class for " & GetType(T).Name)
            End If
        End Sub
        ''' <summary>
        ''' Gets the objecttype.
        ''' </summary>
        ''' <value>The objecttype.</value>
        Public ReadOnly Property Objecttype() As Type
            Get
                Return Me._objecttype
            End Get
        End Property

        ''' <summary>
        ''' Gets the objecttypename.
        ''' </summary>
        ''' <value>The objecttypename.</value>
        Public ReadOnly Property Objecttypename() As String
            Get
                If _objecttype IsNot Nothing Then Return Me._objecttype.Name
                Return ""
            End Get
        End Property

        ''' <summary>
        ''' Gets the no keys.
        ''' </summary>
        ''' <value>The no keys.</value>
        Public ReadOnly Property NoKeys() As UShort
            Get
                Return Me._noKeys
            End Get
        End Property

    End Class

    ''' <summary>
    ''' the generic object unique key class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class DataKeyTuple
        Implements IHashCodeProvider
        Implements IQueryable
        Implements IComparable


        '** Keys is an array of objects
        Protected _Values() As Object

        Private _lockobject As New Object ''' internal lock object

        ''' <summary>
        ''' constructor of an keyentry - creates an objectkey for number of keys (1..)
        ''' </summary>
        ''' <param name="registeryentry"></param>
        ''' <remarks></remarks>

        Public Sub New(nokeys As UShort)
            ReDim _Values(nokeys - 1)
        End Sub
        Public Sub New(keys() As Object)
            _Values = keys
        End Sub
        ''' <summary>
        ''' returns the size of the ObjectKey Array
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Size As UShort
            Get
                Return _Values.GetUpperBound(0) + 1
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the keys.
        ''' </summary>
        ''' <value>The keys.</value>
        Public Overridable Property Values() As Object()
            Get
                Return Me._Values
            End Get
            Set(value As Object())
                If value.GetUpperBound(0) <> _Values.GetUpperBound(0) Then Throw New Exception("keys of this type have different bound")
                ReDim Preserve _Values(value.GetUpperBound(0))
                Me._Values = value
            End Set
        End Property

        ''' <summary>
        ''' returns a hash value for the keys
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function GetHashCode(o As Object) As Integer Implements IHashCodeProvider.GetHashCode
            Dim aKey As DataKeyTuple = TryCast(o, DataKeyTuple)
            If aKey Is Nothing Then Return o.GetHashCode
            If aKey.Values Is Nothing Then Return 0

            Dim hashvalue As Integer = 0
            For i = 0 To aKey.Values.Count - 1
                If aKey.Values(i) Is Nothing Then
                    hashvalue = hashvalue Xor 0
                Else
                    hashvalue = hashvalue Xor aKey.Values(i).GetHashCode()
                End If
            Next
            Return hashvalue
        End Function
        ''' <summary>
        ''' Equal routine of 2 keys
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Equals(obj As Object) As Boolean
            Try
                Dim aKey As DataKeyTuple = TryCast(obj, DataKeyTuple)
                If aKey Is Nothing Then
                    Return False
                Else
                    If (aKey.Values Is Nothing AndAlso _Values IsNot Nothing) OrElse _
                        (aKey.Values IsNot Nothing AndAlso _Values Is Nothing) Then
                        Return False
                    End If
                    If (aKey.Values Is Nothing AndAlso _Values Is Nothing) Then
                        Return True
                    End If

                    If aKey.Values.Count <> _Values.Count Then Return False
                    For i = 0 To aKey.Values.Count - 1
                        If aKey(i).GetType.Equals(Me(i).GetType) Then
                            If aKey(i).Equals(Me(i)) Then Return True
                            Return False
                        Else
                            Try
                                Dim avalue = CTypeDynamic(aKey(i), Me(i).GetType)
                                If Me(i).Equals(avalue) Then Return True

                            Catch ex As Exception
                                Return False
                            End Try

                        End If

                    Next
                    Return True
                End If

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectKeyArray.Equals")
                Return False
            End Try
        End Function
        ''' <summary>
        ''' returns a hash value for the keys
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function GetHashCode() As Integer
            Return Me.GetHashCode(Me)
        End Function
        ''' <summary>
        ''' gets or sets the item in an key
        ''' </summary>
        ''' <param name="index"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Default Public Property Item(index As UShort) As Object
            Get
                Return _Values(index)
            End Get
            Set(value As Object)
                _Values(index) = value
            End Set
        End Property
        ''' <summary>
        ''' get an enumerator
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return _Values.ToList
        End Function

        Public ReadOnly Property ElementType As Type Implements IQueryable.ElementType
            Get

            End Get
        End Property

        Public ReadOnly Property Expression As Expressions.Expression Implements IQueryable.Expression
            Get

            End Get
        End Property

        Public ReadOnly Property Provider As IQueryProvider Implements IQueryable.Provider
            Get

            End Get
        End Property

        ''' <summary>
        ''' toString
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ToString() As String
            If _Values IsNot Nothing Then
                Dim s As String = "["
                For i = 0 To _Values.Count - 1
                    If s <> "[" Then s &= ","
                    s &= _Values(i).ToString
                Next
                Return s & "]"
            Else
                Return "[]"
            End If

        End Function

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Try
                Dim aKey As DataKeyTuple = TryCast(obj, DataKeyTuple)
                If aKey Is Nothing Then
                    Return False
                Else
                    If (aKey.Values Is Nothing AndAlso _Values IsNot Nothing) Then
                        Return 1
                    ElseIf (aKey.Values IsNot Nothing AndAlso _Values Is Nothing) Then
                        Return -1
                    End If
                    If (aKey.Values Is Nothing AndAlso _Values Is Nothing) Then
                        Return 0
                    End If

                    If aKey.Values.Count <> _Values.Count Then Return False
                    Dim result As Integer = 0
                    For i = 0 To aKey.Values.Count - 1
                        If Not aKey(i).Equals(Me(i)) Then
                            '' compare them if we can
                            If (aKey.GetType.GetInterfaces.Contains(GetType(IComparable))) AndAlso (_Values(i).GetType.GetInterfaces.Contains(GetType(IComparable))) Then
                                Return TryCast(_Values(i), IComparable).CompareTo(TryCast(aKey(i), IComparable))
                            Else
                                Return _Values(i).ToString.CompareTo(aKey(i).ToString)
                            End If
                        End If
                    Next
                    Return result
                End If

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectKeyArray.Equals")
                Return False
            End Try
        End Function
    End Class

    ''' <summary>
    ''' the generic object unique key class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ormPrimaryKey(Of T)
        Inherits DataKeyTuple


        '** Keys is an array of objects
        Private _registery As ormDataTupleMetaClass(Of T)

        ''' <summary>
        ''' constructor of an keyentry
        ''' </summary>
        ''' <param name="registeryentry"></param>
        ''' <remarks></remarks>
        Public Sub New(registeryentry As ormDataTupleMetaClass(Of T))
            MyBase.New(registeryentry.NoKeys)
            _registery = registeryentry
        End Sub
        Public Sub New(keyvalues() As Object)
            MyBase.New(keyvalues)
        End Sub
        ''' <summary>
        ''' Gets or sets the keys.
        ''' </summary>
        ''' <value>The keys.</value>
        Public Overrides Property Values() As Object()
            Get
                Return Me._Values
            End Get
            Set(value As Object())
                If value.GetUpperBound(0) <> _registery.NoKeys - 1 Then Throw New Exception("keys of this type have different bound")
                Me._Values = value
            End Set
        End Property

    End Class


    ''' <summary>
    ''' represents a record data tuple for to be stored and retrieved in a data store
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ormRecord
        Inherits Dynamic.DynamicObject

        Private _FixEntries As Boolean = False
        Private _isBound As Boolean = False
        Private _TableStores As iormDataStore() = {}
        Private _DbDriver As iormDatabaseDriver = Nothing
        Private _entrynames() As String = {}
        Private _Values() As Object = {}
        Private _OriginalValues() As Object = {}
        Private _isCreated As Boolean = False
        Private _isUnknown As Boolean = True
        Private _isLoaded As Boolean = False
        Private _isChanged As Boolean = False
        Private _tableids As String() = {}
        Private _upperRangeofTable As ULong() = {}
        Private _isnullable As Boolean() = {}

        '** initialize
        Public Sub New()

        End Sub

        Public Sub New(ByVal tableID As String, _
                       Optional dbdriver As iormDatabaseDriver = Nothing, _
                       Optional fillDefaultValues As Boolean = False, _
                       Optional runtimeOnly As Boolean = False)
            _DbDriver = dbdriver
            ReDim _tableids(0)
            _tableids(0) = tableID
            If Not runtimeOnly Then
                Me.SetTable(tableID, forceReload:=False, dbdriver:=dbdriver, fillDefaultValues:=fillDefaultValues)
                _FixEntries = True
            End If
        End Sub

        Public Sub New(ByVal tableIDs As String(), _
                       Optional dbdriver As iormDatabaseDriver = Nothing, _
                       Optional fillDefaultValues As Boolean = False, _
                       Optional runtimeOnly As Boolean = False)
            _DbDriver = dbdriver
            _tableids = tableIDs
            If Not runtimeOnly Then
                Me.SetTables(tableIDs, forceReload:=False, dbdriver:=dbdriver, fillDefaultValues:=fillDefaultValues)
                _FixEntries = True
            End If
        End Sub

        Public Sub Finalize()
            _DbDriver = Nothing
            _TableStores = Nothing
            _Values = Nothing
            _OriginalValues = Nothing
        End Sub

        ' If you try to get a value of a property that is
        ' not defined in the class, this method is called.
        ''' <summary>
        ''' dynamic getValue Property
        ''' </summary>
        ''' <param name="binder"></param>
        ''' <param name="result"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function TryGetMember(
            ByVal binder As System.Dynamic.GetMemberBinder,
            ByRef result As Object) As Boolean

            ' Converting the property name to lowercase
            ' so that property names become case-insensitive.
            Dim name As String = binder.Name

            ' If the property name is found in a dictionary,
            ' set the result parameter to the property value and return true.
            ' Otherwise, return false.
            Dim flag As Boolean
            result = Me.GetValue(index:=name, notFound:=flag)
            Return flag
        End Function
        ''' <summary>
        ''' Dynamic setValue Property
        ''' </summary>
        ''' <param name="binder"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function TrySetMember(
            ByVal binder As System.Dynamic.SetMemberBinder,
            ByVal value As Object) As Boolean

            ' Converting the property name to lowercase
            ' so that property names become case-insensitive.
            Return Me.SetValue(index:=binder.Name, value:=value)

        End Function
        ''' <summary>
        ''' Gets the is table set.
        ''' </summary>
        ''' <value>The is table set.</value>
        Public ReadOnly Property IsBound() As Boolean
            Get
                Return Me._isBound
            End Get
        End Property

        ''' <summary>
        ''' set if this record is a new Record in the databse
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsCreated As Boolean
            Get
                Return _isCreated
            End Get
            Friend Set(value As Boolean)

                If value Then
                    _isCreated = True
                    _isLoaded = False
                    _isUnknown = False
                End If
            End Set
        End Property
        ''' <summary>
        ''' set if the record state is unkown if new or load
        ''' </summary>
        ''' <value>The is unknown.</value>
        Public Property IsUnknown() As Boolean
            Get
                Return Me._isUnknown
            End Get
            Set(value As Boolean)
                Me._isUnknown = value
                If value Then
                    Me.iscreated = False
                    Me.isloaded = False
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the is changed.
        ''' </summary>
        ''' <value>The is changed.</value>
        Public Property IsChanged() As Boolean
            Get
                Return Me._isChanged
            End Get
            Friend Set(value As Boolean)
                Me._isChanged = value
            End Set
        End Property
        ''' <summary>
        ''' set if record is loaded
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsLoaded As Boolean
            Get
                Return _isLoaded
            End Get
            Friend Set(value As Boolean)
                If value Then
                    _isCreated = False
                    _isLoaded = True
                    _isUnknown = False
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns true if record is alive
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Alive As Boolean
            Get
                If _FixEntries Then
                    Return _isBound
                Else
                    Return True
                End If

            End Get
        End Property
        ''' <summary>
        ''' returns Length of Record
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Length As Integer
            Get
                Length = UBound(_Values)
            End Get
        End Property
        ''' <summary>
        '''  the TableID to the Record
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property TableIDs As String()
            Get
                Return TableIDs
            End Get
            Private Set(value As String())
                If Not _isBound Then
                    _tableids = value
                Else
                    CoreMessageHandler(message:="tableids cannot be assigned after binding a record", subname:="ormRecord.tableids")
                    Throw New ormException(message:="tableids cannot be assigned after binding a record")
                End If
            End Set
        End Property

        ''' <summary>
        ''' returns the tablestore for the tableid if bound
        ''' </summary>
        ''' <param name="tableid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetTablestore(tableid As String) As iormDataStore
            If _isBound Then
                Dim i As Integer = Array.IndexOf(_tableids, tableid.ToUpper)
                If i >= 0 Then Return _TableStores(i)
                Return Nothing
            Else
                Return Nothing
            End If

        End Function
        ''' <summary>
        ''' returns the tablestores or nothing
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TableStores As iormDataStore()
            Get
                If Alive Then
                    Return _TableStores
                Else
                    Return Nothing
                End If
            End Get

        End Property

        ''' <summary>
        ''' returns the values
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Values As List(Of Object)
            Get
                Return _Values.ToList
            End Get
        End Property

        ''' <summary>
        ''' load a record into this record from the datareader
        ''' </summary>
        ''' <param name="datareader"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadFrom(ByRef datarow As DataRow) As Boolean
            Dim result As Boolean = True
            Try
                ''' if tableset then only check which fields are in the datareader
                ''' 
                _isLoaded = True ' important

                If _isBound Then
                    Dim flagColumnNameCheck As Boolean = False
                    If _TableStores.Length > 1 OrElse datarow.Table.TableName.ToUpper <> _TableStores(0).TableSchema.TableID.ToUpper Then
                        flagColumnNameCheck = True
                    End If

                    ''' run through
                    For n = 0 To _TableStores.Length - 1
                        For j = 1 To _TableStores(n).TableSchema.NoFields
                            Dim aColumnname As String = _TableStores(n).TableSchema.Getfieldname(j)
                            If datarow.Table.Columns.Contains(aColumnname) Then
                                Dim aValue As Object = datarow.Item(aColumnname)
                                If flagColumnNameCheck AndAlso ZeroBasedIndexOf(_TableStores(n).TableID & "." & aColumnname) < 0 Then
                                    CoreMessageHandler(message:="column doesnot exist in record ?!", arg1:=datarow.Item(aColumnname), _
                                                        columnname:=aColumnname, tablename:=datarow.Table.TableName, subname:="ormRecord.LoadFrom(Datarow)")
                                    '''convert and set the value
                                ElseIf _TableStores(n).Convert2ObjectData(index:=j, invalue:=datarow.Item(aColumnname), outvalue:=aValue) Then
                                    If Not SetValue(j, aValue) Then
                                        CoreMessageHandler(message:="could not set value from data reader", arg1:=aValue, _
                                                           columnname:=aColumnname, tablename:=datarow.Table.TableName, subname:="ormRecord.LoadFrom(Datarow)")
                                        result = False
                                    Else
                                        result = result And True
                                    End If
                                Else
                                    CoreMessageHandler(message:="could not convert value from data reader", arg1:=datarow.Item(aColumnname), _
                                                       columnname:=aColumnname, tablename:=datarow.Table.TableName, subname:="ormRecord.LoadFrom(Datarow)")
                                    result = False
                                End If

                            Else
                                CoreMessageHandler(message:="column from table not in datareader - record uncomplete", columnname:=aColumnname, _
                                                   tablename:=datarow.Table.TableName, subname:="ormRecord.LoadFrom(Datarow)")
                                result = False
                            End If
                        Next j
                    Next


                    Return result
                Else
                    ''' take all the values from datareader and move it 
                    ''' 
                    For j = 0 To datarow.Table.Columns.Count - 1
                        Dim aColumnname As String = datarow.Table.Columns.Item(j).ColumnName
                        Dim aValue As Object = datarow.Item(j)

                        ''' how to convert ?!
                        ''' 
                        ''' datarow has system types !!
                        ''' Dim Outvalue = CTypeDynamic (avalue, atype)
                        '''
                        If Not SetValue(datarow.Table.TableName.ToUpper & "." & aColumnname.ToUpper, aValue) Then
                            CoreMessageHandler(message:="could not set value from data reader", arg1:=aValue, _
                                               columnname:=aColumnname, tablename:=datarow.Table.TableName, subname:="ormRecord.LoadFrom(Datarow)")
                            result = False
                        Else
                            result = True
                        End If
                    Next

                    Return result
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="ormRecord.LoadFrom(Datarow)", exception:=ex, message:="Exception", tablename:=datarow.Table.TableName)
                Return False
            End Try

        End Function

        ''' <summary>
        ''' load a record into this record from the datareader
        ''' </summary>
        ''' <param name="datareader"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadFrom(ByRef datareader As IDataReader) As Boolean
            Dim result As Boolean = True

            Try
                ''' if tableset then only check which fields are in the datareader
                ''' 
                _isLoaded = True ' important

                If _isBound Then
                    ''' go through each tablestore
                    For n = 0 To _TableStores.Length - 1
                        For j = 1 To _TableStores(n).TableSchema.NoFields
                            Dim found As Integer = -1
                            Dim aColumnname As String = _TableStores(n).TableSchema.Getfieldname(j)
                            For i = 0 To datareader.FieldCount - 1
                                If datareader.GetName(i) = aColumnname Then
                                    ''' uuuh slow
                                    ''' 
                                    found = i
                                    Exit For
                                End If
                            Next
                            If found >= 0 Then
                                Dim aValue As Object
                                Dim index As Integer = ZeroBasedIndexOf(_TableStores(n).TableID & "." & aColumnname) + 1
                                If index >= 0 Then
                                    If _TableStores(n).Convert2ObjectData(index:=j, invalue:=datareader.Item(found), outvalue:=aValue) Then
                                        If Not SetValue(index, aValue) Then
                                            CoreMessageHandler(message:="set value failed", arg1:=aValue, columnname:=aColumnname, tablename:=_tableids(n), subname:="ormRecord.LoadFrom")
                                            result = False
                                        Else
                                            result = result And True
                                        End If
                                    Else
                                        CoreMessageHandler(message:="data conversion failed", arg1:=datareader.Item(aColumnname), columnname:=aColumnname, _
                                                           tablename:=_tableids(n), subname:="ormRecord.LoadFrom")
                                        result = False
                                    End If
                                Else
                                    CoreMessageHandler(message:="index in record failed - canonical name doesnot exist ?", _
                                                       arg1:=datareader.Item(aColumnname), columnname:=aColumnname, tablename:=_tableids(n), subname:="ormRecord.LoadFrom")
                                    result = False
                                End If

                            Else
                                CoreMessageHandler(message:="column from table not in datareader - record uncomplete", columnname:=aColumnname, _
                                                   tablename:=_tableids(n), subname:="ormRecord.LoadFrom(IDataReader)")
                                result = False
                            End If
                        Next j
                    Next


                    Return result
                Else
                    ''' take all the values from datareader and move it 
                    ''' 
                    For j = 0 To datareader.FieldCount - 1
                        Dim aName As String = datareader.GetName(j)
                        If aName = "" Then aName = "column" & j.ToString
                        Dim aValue As Object = datareader.Item(j)

                        ''' how to convert ?!
                        ''' we have already system type

                        If Not SetValue(aName.ToString, aValue) Then
                            CoreMessageHandler(message:="could not set value from data reader", arg1:=aValue, _
                                                messagetype:=otCoreMessageType.InternalError, subname:="ormRecord.LoadFrom(IDataReader)")
                            result = False
                        Else
                            result = result And True
                        End If
                    Next

                    Return result
                End If


            Catch ex As Exception
                Call CoreMessageHandler(subname:="ormRecord.LoadFrom(IDataReader)", exception:=ex, message:="Exception", _
                                      arg1:=_tableids)
                Return False
            End Try

        End Function

        ''' <summary>
        ''' checkStatus if loaded or created by checking if Record exists in Table. Sets the isChanged / isLoaded Property
        ''' </summary>
        ''' <returns>true if successfully checked</returns>
        ''' <remarks></remarks>
        Public Function CheckStatus(Optional ByRef status As Boolean() = Nothing) As Boolean
            Dim aLoad As Boolean = False
            Dim aCreate As Boolean = False

            '** not loaded and not created but alive ?!
            If Not Me.IsLoaded AndAlso Not Me.IsCreated AndAlso Alive Then

                ReDim status(_tableids.Length - 1)
                For n = 0 To _TableStores.Length - 1
                    Dim pkarr() As Object
                    Dim i, index As Integer
                    Dim value As Object

                    Dim aRecord As ormRecord
                    Try
                        ReDim pkarr(0 To _TableStores(n).TableSchema.NoPrimaryKeyFields - 1)
                        For i = 1 To _TableStores(n).TableSchema.NoPrimaryKeyFields
                            index = _TableStores(n).TableSchema.GetordinalOfPrimaryKeyField(i)
                            value = Me.GetValue(index)
                            pkarr(i - 1) = value
                        Next i
                        ' delete
                        aRecord = _TableStores(n).GetRecordByPrimaryKey(pkarr)
                        status(n) = aRecord IsNot Nothing

                        If aRecord Is Nothing Then
                            aCreate = True
                        Else
                            aLoad = True
                        End If
                    Catch ex As Exception
                        Call CoreMessageHandler(exception:=ex, message:="Exception", messagetype:=otCoreMessageType.InternalException, _
                                              subname:="ormRecord.checkStatus")
                        Return False
                    End Try
                Next

                If aLoad And Not aCreate Then
                    Me.IsLoaded = True
                ElseIf aCreate And Not aLoad Then
                    Me.IsCreated = True
                Else
                    Me.IsUnknown = True
                    'not determinable
                End If

            End If


            Return True
        End Function

        ''' <summary>
        ''' sets the default value to an index
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="tableid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetDefaultValue(index As Object) As Object
            Dim i As Integer
            ''' only on bound
            ''' 
            If Not Me.Alive Or Not Me.IsBound Then
                Return Nothing
            End If

            If IsNumeric(index) Then
                i = CInt(index) - 1
            Else
                i = ZeroBasedIndexOf(index)
                If i < 0 Then
                    Return Nothing
                End If
            End If

            ' prevent overflow
            If Not (i > 0 And i <= _Values.Count) Then
                Return Nothing
            End If

            '* set the default values
            '* do not allow recursion on objectentrydefinition table itself
            '* since this is not included 

            Dim names As String() = index.ToString.ToUpper.Split({CChar(ConstDelimiter), "."c})
            Dim n As Integer = Array.IndexOf(_tableids, names(0))
            If n >= 0 Then
                Return _TableStores(n).TableSchema.GetDefaultValue(i)
            Else
                Return Nothing
            End If

        End Function

        ''' <summary>
        ''' set the table of this records and bind it to it
        ''' </summary>
        ''' <param name="tableID"></param>
        ''' <param name="dbdriver"></param>
        ''' <param name="tablestore"></param>
        ''' <param name="forceReload"></param>
        ''' <param name="fillDefaultValues"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Function SetTable(ByVal tableID As String, _
                                 Optional dbdriver As iormDatabaseDriver = Nothing, _
                                 Optional tablestore As iormDataStore = Nothing, _
                                 Optional forceReload As Boolean = False, _
                                 Optional fillDefaultValues As Boolean = False) As Boolean
            Return Me.SetTables(tableIDs:={tableID}, dbdriver:=dbdriver, forceReload:=forceReload, fillDefaultValues:=fillDefaultValues)
        End Function


        ''' <summary>
        ''' set the tables of this record and bind it to them !
        ''' </summary>
        ''' <param name="TableID">Name of the Table</param>
        ''' <param name="ForceReload">Forece to reaassign</param>
        ''' <returns>True if ssuccessfull</returns>
        ''' <remarks></remarks>
        ''' 
        Public Function SetTables(ByVal tableIDs() As String, _
                                 Optional dbdriver As iormDatabaseDriver = Nothing, _
                                 Optional forceReload As Boolean = False, _
                                 Optional fillDefaultValues As Boolean = False) As Boolean

            If Not _isBound Or forceReload Then

                ReDim _TableStores(tableIDs.Length - 1)
                ReDim _upperRangeofTable(tableIDs.Length - 1)
                Dim totalsize As ULong = 0

                ''' PHASE I: get the tablestores
                '''
                For I = 0 To _TableStores.Length - 1
                    If dbdriver Is Nothing Then
                        _TableStores(I) = ot.GetTableStore(tableIDs(I))
                    Else
                        _TableStores(I) = dbdriver.GetTableStore(tableIDs(I))
                    End If

                    If _TableStores(I) Is Nothing OrElse _TableStores(I).TableSchema Is Nothing _
                        OrElse Not _TableStores(I).TableSchema.IsInitialized Then

                        CoreMessageHandler(message:="record cannot be bound to table - tablestore cannot be initialized", arg1:=tableIDs(I), _
                                           subname:="ormRecord.setTables")
                        Return False
                    Else
                        '' set the upper ranges in the record
                        _upperRangeofTable(I) = _TableStores(I).TableSchema.NoFields - 1
                        totalsize += _upperRangeofTable(I)
                    End If

                Next I

                ''' PHASE II : resize the internals
                ''' 
                '*** redim else and set the default values
                ReDim Preserve _Values(totalsize)
                ReDim Preserve _OriginalValues(totalsize)
                ReDim Preserve _isnullable(totalsize)
                'ReDim Preserve _entrynames(totalsize) ' not here we rely on _entrynames to see if we are used before binding
                _tableids = tableIDs

                ''' set the values and entries
                _isBound = True
                _FixEntries = True

                ' get the number of fields
                If totalsize > 0 Then

                    '*** if there have been entries before or was set to another table
                    '*** preserve as much as possible
                    If _entrynames.GetUpperBound(0) > 0 Then

                        Dim newValues(totalsize) As Object
                        Dim newOrigValues(totalsize) As Object
                        Dim newEntrynames(totalsize) As String

                        For I = 0 To _TableStores.Length - 1
                            Dim aTablename As String = _TableStores(I).TableID.ToUpper
                            '** re-sort 
                            For j = 1 To _TableStores(I).TableSchema.NoFields

                                ''' calculate new index
                                Dim index As UShort = 0
                                If I > 0 Then index = _upperRangeofTable(I - 1)
                                index += j - 1
                                Dim aFieldname As String = _TableStores(I).TableSchema.Getfieldname(j).ToUpper
                                Dim aCanonicalName As String = aTablename & "." & aFieldname
                                newEntrynames(index) = aCanonicalName
                                ''' fill the nullable
                                _isnullable(index) = _TableStores(I).TableSchema.GetNullable(j)
                                '' get old index 
                                Dim oldindex As Integer = Array.FindIndex(_entrynames, Function(x) x IsNot Nothing AndAlso (x.ToUpper = aFieldname OrElse x.ToUpper = aCanonicalName))
                                If oldindex >= 0 Then
                                    newValues(index) = _Values(oldindex)
                                    newOrigValues(index) = _Values(oldindex)
                                Else
                                    ' can be - default value ? CoreMessageHandler(message:="index not found", subname:="ormRecord.SetTables", messagetype:=otCoreMessageType.InternalError)
                                End If

                            Next
                        Next

                        '** change over
                        _Values = newValues
                        _OriginalValues = newOrigValues
                        _entrynames = newEntrynames
                    Else
                        ReDim Preserve _entrynames(totalsize)
                        ''' set the entry names and initial values
                        ''' for each table
                        For I = 0 To _TableStores.Length - 1
                            For j = 1 To _TableStores(I).TableSchema.NoFields
                                ''' calculate index
                                Dim index As UShort = 0
                                If I > 0 Then index = _upperRangeofTable(I - 1)
                                index += j - 1
                                ''' set fieldname
                                _entrynames(index) = _TableStores(I).TableID.ToUpper & "." & _TableStores(I).TableSchema.Getfieldname(j).ToUpper
                                ''' fill the nullable
                                _isnullable(index) = _TableStores(I).TableSchema.GetNullable(j)
                                ''' fill default from tablestore
                                If fillDefaultValues Then
                                    If Not _TableStores(I).TableSchema.GetNullable(j) Then
                                        _Values(index) = Me.GetDefaultValue(j)
                                    Else
                                        _Values(index) = Nothing
                                    End If
                                End If
                                ''' set the orginal values with default values
                                _OriginalValues(index) = _Values(index)

                            Next
                        Next

                    End If

                    Return _isBound

                Else
                    Call CoreMessageHandler(message:="Tablestore or tableschema is not initialized", subname:="ormRecord.setTables", _
                                          messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If

                Return False
            Else
                Return True 'already set
            End If
        End Function
        ''' <summary>
        ''' persists the Record in the Database
        ''' </summary>
        ''' <param name="aTimestamp">Optional TimeStamp for using the persist</param>
        ''' <returns>true if successfull</returns>
        ''' <remarks></remarks>
        Public Function Persist(Optional ByVal timestamp As Date = ot.ConstNullDate) As Boolean
            Dim result As Boolean = True
            Dim aStatus As Boolean()
            '** try to set the table
            If Not _isBound And _tableids.Length <> 0 Then
                Me.SetTables(tableIDs:=_tableids)
            End If
            '** only on success
            If _isBound Then
                If timestamp = ConstNullDate Then timestamp = Date.Now
                '' check for status
                If Not Me.IsCreated AndAlso Not Me.IsLoaded Then CheckStatus(aStatus)
                '* persist in each store
                For i = 0 To _TableStores.Length - 1
                    result = result And _TableStores(i).PersistRecord(Me, timestamp:=timestamp)
                Next i
                '* result
                If result Then
                    Me.IsLoaded = True
                    Me.IsCreated = False
                    Me.IsChanged = False
                    Return True
                End If
            Else
                CoreMessageHandler(message:="unbound record cannot be persisted", messagetype:=otCoreMessageType.InternalError, subname:="ormRecord.Persist")
                Return False
            End If
        End Function

        ''' <summary>
        ''' Deletes the Record in all tablestores
        ''' </summary>
        ''' <returns>true if successfull</returns>
        ''' <remarks></remarks>

        Public Function Delete() As Boolean
            Dim pkarr() As Object
            Dim i, index As Integer
            Dim result As Boolean = True

            If _isBound Then
                For n = 0 To _TableStores.Length - 1
                    ReDim pkarr(0 To _TableStores(n).TableSchema.NoPrimaryKeyFields - 1)
                    For i = 0 To _TableStores(n).TableSchema.NoPrimaryKeyFields - 1
                        ''' get index
                        If n > 0 Then
                            index = _upperRangeofTable(n - 1)
                        Else
                            index = 0
                        End If
                        index += _TableStores(n).TableSchema.GetordinalOfPrimaryKeyField(i + 1)
                        If Me.HasIndex(index) Then
                            pkarr(i) = Me.GetValue(index)
                        Else
                            CoreMessageHandler(message:="part of primary key for tablestore is not in record", columnname:=index, _
                                               tablename:=_TableStores(n).TableID, subname:="ormRecord.Delete", messagetype:=otCoreMessageType.InternalError)
                        End If

                    Next i
                    ' delete
                    result = result And _TableStores(n).DelRecordByPrimaryKey(pkarr)
                Next
                Return result
            Else
                Call CoreMessageHandler(subname:="ormRecord.delete", message:="Record not bound to a TableStore", _
                                      messagetype:=otCoreMessageType.InternalError)
                Return False
            End If
            Return False
        End Function
        ''' <summary>
        ''' returns true if the record has the index either numerical (1..) or by name
        ''' a tablename in form [tablename].[columnname] will be stripped of and checked too 
        ''' </summary>
        ''' <param name="anIndex"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasIndex(index As Object) As Boolean
            If IsNumeric(index) Then
                Dim i = CInt(index) - 1
                If i >= LBound(_Values) And i <= UBound(_Values) Then
                    Return True
                Else
                    Return False
                End If
            Else
                If ZeroBasedIndexOf(index) >= 0 Then Return True
            End If

        End Function

        ''' <summary>
        ''' retus a list of the primaryKeys
        ''' </summary>
        ''' <returns>List(of String)</returns>
        ''' <remarks></remarks>
        Public Function Keys() As List(Of String)
            ' no table ?!
            If Not Me.Alive Then
                Return New List(Of String)
            ElseIf _isBound And _entrynames.Length = 0 Then
                Dim aList As New List(Of String)
                For n = 0 To _TableStores.Length - 1
                    aList.AddRange(_TableStores(n).TableSchema.Fieldnames)
                Next
            Else
                Keys = _entrynames.ToList
            End If
        End Function




        ''' <summary>
        ''' gets the index of an entryname 0 ... n !!
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ZeroBasedIndexOf(entryname As String) As Integer
            entryname = entryname.ToUpper
            Dim i As Integer = Array.IndexOf(_entrynames, entryname.ToUpper)
            If i < 0 Then
                Dim names As String() = entryname.ToUpper.Split({CChar(ConstDelimiter), "."c})
                If names.Count > 1 Then
                    If _isBound Then
                        i = 0
                        Dim n As Integer = Array.IndexOf(_tableids, names(0))
                        If n < 0 Then Return -1
                        If n > 0 Then i = _upperRangeofTable(n - 1)
                        i += _TableStores(n).TableSchema.GetFieldordinal(names(1)) - 1
                        Return i
                    Else
                        Dim acolumnname As String = entryname.Split({CChar(ConstDelimiter), "."c}).Last
                        Return Array.FindIndex(_entrynames, Function(x) x IsNot Nothing AndAlso (x.ToUpper = entryname OrElse x = acolumnname OrElse entryname = x.Split({CChar(ConstDelimiter), "."c}).Last.ToUpper))
                    End If
                Else

                    Return Array.FindIndex(_entrynames, Function(x) x IsNot Nothing AndAlso (x.ToUpper = entryname.ToUpper OrElse entryname.ToUpper = x.Split({CChar(ConstDelimiter), "."c}).Last.ToUpper))
                End If

            Else
                Return i 'if found or not bound
            End If

        End Function
        ''' <summary>
        ''' returns True if Value of anIndex is Changed
        ''' </summary>
        ''' <param name="anIndex">index in Number 1..n or fieldname</param>
        ''' <returns>True on Change</returns>
        ''' <remarks></remarks>
        Public Function IsValueChanged(ByVal index As Object) As Boolean
            Dim i As Integer

            ' no table ?!
            If Not _isBound Then
                Call CoreMessageHandler(subname:="ormRecord.isValueChanged", arg1:=index, message:="record is not bound to table")
                Return False
            End If

            If IsNumeric(index) Then
                i = CInt(index) - 1
            Else
                i = ZeroBasedIndexOf(index)
                If i < 0 Then Return False
            End If
            ' set the value
            If (i) >= LBound(_Values) And (i) <= UBound(_Values) Then
                If (Not _OriginalValues(i) Is Nothing AndAlso Not _OriginalValues(i).Equals(_Values(i)) _
                    OrElse IsCreated) Then
                    Return True
                Else
                    _isChanged = _isChanged And False
                    Return False
                End If

            Else

                Call CoreMessageHandler(message:="Index of " & index & " is out of bound of OTDBTableEnt ", _
                                      subname:="ormRecord.isIndexChangedValue", arg1:=index, _
                                      messagetype:=otCoreMessageType.InternalError)
                Return False
            End If

        End Function
        ''' <summary>
        ''' sets the record to an array
        ''' </summary>
        ''' <param name="array"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function [Set](ByRef [array] As Object(), Optional ByRef names As Object() = Nothing) As Boolean
            ' no table ?!
            If Not Me.Alive Then
                Return False
            End If
            '** fixed ?!
            Try
                If _Values.GetUpperBound(0) > 0 Then
                    If [array].GetUpperBound(0) <> _Values.GetUpperBound(0) Then
                        CoreMessageHandler(message:="input array has different upper bound than the set values array", arg1:=[array].GetUpperBound(0), _
                                            messagetype:=otCoreMessageType.InternalError)
                        Return False
                    Else
                        _OriginalValues = _Values.Clone
                        _Values = [array].Clone
                        If Not names Is Nothing Then
                            _entrynames = names.Clone
                        End If
                        Return True
                    End If
                Else
                    ReDim _Values([array].Length)
                    ReDim _OriginalValues([array].Length)
                    _Values = [array].Clone
                    If Not names Is Nothing Then
                        _entrynames = names.Clone
                    End If
                End If

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormRecord.Set")
                Return False
            End Try



        End Function


        ''' <summary>
        ''' set the Value of an Entry of the Record
        ''' </summary>
        ''' <param name="anIndex">Index as No 1...n or name or [tablename].[columnname]</param>
        ''' <param name="anValue">value</param>
        ''' <param name="FORCE"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetValue(ByVal index As Object, ByVal value As Object, Optional ByVal force As Boolean = False) As Boolean
            Dim i As Integer

            Try
                ' no table ?!
                If Not Me.Alive And Not force Then
                    SetValue = False
                    Exit Function
                End If
                '*
                If DBNull.Value.Equals(value) Then
                    value = Nothing
                End If

                If IsNumeric(index) Then
                    i = CLng(index) - 1
                    If i > _entrynames.GetUpperBound(0) OrElse i < 0 Then
                        CoreMessageHandler(message:="index is out of range 0.." & _entrynames.GetUpperBound(0), arg1:=i, _
                                            messagetype:=otCoreMessageType.InternalError, subname:="ormRecord.SetValue")
                        Return False  'wrong table
                    End If

                Else
                    i = ZeroBasedIndexOf(index)
                    If i < 0 And _isBound Then
                        CoreMessageHandler(message:="column name was not found as index in record", arg1:=index, _
                                            messagetype:=otCoreMessageType.InternalError, subname:="ormRecord.SetValue")
                        Return False  'wrong table
                    End If

                End If
                '*** else dynamic extend

                '** extend if not found
                If i < 0 Then
                    i = _entrynames.GetUpperBound(0) + 1

                    ReDim Preserve _entrynames(i)
                    ReDim Preserve _Values(i)
                    ReDim Preserve _OriginalValues(i)
                    ReDim Preserve _isnullable(i)

                    If index.ToString.Contains("."c) OrElse index.ToString.Contains(ConstDelimiter) Then
                        _entrynames(i) = index.ToString.ToUpper
                    ElseIf _tableids.Count = 1 Then
                        _entrynames(i) = _tableids(0) & "." & index.ToString.ToUpper
                    Else
                        _entrynames(i) = index.ToString.ToUpper
                    End If

                    _isnullable(i) = True
                End If

                '''' set the value
                '''

                If (i) >= LBound(_Values) And (i) <= UBound(_Values) Then
                    ' save old value
                    _OriginalValues(i) = _Values(i)
                    ' condition to accept nothing
                    If (value Is Nothing AndAlso _isnullable(i)) Then
                        _Values(i) = Nothing
                    ElseIf value Is Nothing AndAlso _isnullable(i) AndAlso Reflector.IsNullableTypeOrString(value) Then
                        _Values(i) = Nothing
                    ElseIf value Is Nothing And Not _isnullable(i) Then
                        _Values(i) = GetDefaultValue(i)
                    Else
                        If (value.GetType.GetInterfaces.Contains(GetType(ICloneable))) Then
                            _Values(i) = value.clone
                        Else
                            _Values(i) = value
                        End If
                    End If

                    If _OriginalValues(i) Is Nothing Then
                        _isChanged = False
                    ElseIf (Not _OriginalValues(i) Is Nothing And Not _Values(i) Is Nothing) _
                        AndAlso ((_OriginalValues(i).GetType().Equals(_Values(i)) AndAlso _OriginalValues(i) <> _Values(i))) _
                        OrElse (Not _OriginalValues(i).GetType().Equals(_Values(i))) Then
                        _isChanged = True
                    ElseIf (Not _OriginalValues(i) Is Nothing And _Values(i) Is Nothing) Then
                        _isChanged = True
                    End If
                Else

                    Call CoreMessageHandler(message:="Index of " & index & " is out of bound of", _
                                          subname:="ormRecord.setValue", arg1:=value, entryname:=index, messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If

                Return True


            Catch ex As Exception
                Call CoreMessageHandler(subname:="ormRecord.setValue", exception:=ex)
                Return False
            End Try


        End Function
        ''' <summary>
        ''' returns True if the indexed entry in the record is null or doesnot exist
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsNull(index As Object) As Boolean
            Dim nullvalue As Boolean
            Dim notfound As Boolean
            If Not Me.HasIndex(index:=index) Then Return False
            Dim avalue As Object = Me.GetValue(index:=index, isNull:=nullvalue, notFound:=notfound)
            Return nullvalue
        End Function
        ''' <summary>
        ''' gets the Value of an Entry of the Record
        ''' </summary>
        ''' <param name="anIndex">Index 1...n or name of the Field</param>
        ''' <returns>the value as object or Null of not found</returns>
        ''' <remarks></remarks>
        Public Function GetValue(index As Object, Optional ByRef isNull As Boolean = False, Optional ByRef notFound As Boolean = False) As Object
            Dim i As Long

            Try

                ' no table ?!
                If Not Me.Alive Then
                    GetValue = False
                    Exit Function
                End If


                If IsNumeric(index) Then
                    i = CLng(index) - 1
                Else
                    i = ZeroBasedIndexOf(index)
                    If i < 0 Then
                        CoreMessageHandler(message:="column name could not be found", arg1:=index, _
                                            messagetype:=otCoreMessageType.InternalError, subname:="ormRecord.GetValue")
                        notFound = True
                        Return Nothing  'wrong table
                    End If
                End If


                ''' Get the value
                ''' 
                If (i) >= LBound(_Values) And (i) <= UBound(_Values) Then
                    If DBNull.Value.Equals(_Values(i)) OrElse (_isnullable(i) = True AndAlso _Values(i) Is Nothing) Then
                        isNull = True
                        Return Nothing
                    Else
                        isNull = False
                        Return _Values(i)
                    End If
                Else
                    Call CoreMessageHandler(message:="Index of " & index & " is out of bound of tablestore or doesnot exist in record '", _
                                          subname:="ormRecord.getValue", entryname:=index, messagetype:=otCoreMessageType.InternalError)
                    notFound = True
                    Return Nothing
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="ormRecord.getValue", exception:=ex)
                Return Nothing
            End Try
        End Function

    End Class


End Namespace