
REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** Object Relationship Model Declaration
REM *********** 
REM *********** Version: 2.00
REM *********** Created: 2015-02-13
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2015
REM ***********************************************************************************************************************************************
Option Explicit On
Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports System.Diagnostics
Imports System.Diagnostics.Debug
Imports OnTrack

Namespace OnTrack.Database

    ''' <summary>
    ''' Point of Lifecycle to infuse a relation
    ''' </summary>
    ''' <remarks></remarks>

    Public Enum otInfuseMode
        None = 0
        OnInject = 1
        OnCreate = 2
        OnDefault = 8
        OnDemand = 16
        Always = 27 ' Logical AND of everything
    End Enum
    ''' <summary>
    ''' the Foreign Key Implementation layer
    ''' on Native Database layer or ORM (internal)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otForeignKeyImplementation
        None = 0
        NativeDatabase = 1
        ORM = 3
    End Enum

    ''' <summary>
    ''' Data Types for OnTrack Database Fields
    ''' </summary>
    ''' <remarks></remarks>

    <TypeConverter(GetType(Long))> Public Enum otDataType
        Numeric = 1
        List = 2
        Text = 3
        Runtime = 4
        Formula = 5
        [Date] = 6
        [Long] = 7
        Timestamp = 8
        Bool = 9
        Memo = 10
        Binary = 11
        Time = 12
    End Enum
    ''' <summary>
    ''' Entry Type
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otObjectEntryType
        ContainerEntry = 1
        Compound = 2
    End Enum

    ''' <summary>
    ''' Interface for Object Entries
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface iormObjectEntry
        'Inherits iormPersistable -> ObjectEntryAttribute is also covering this
        'Inherits System.ComponentModel.INotifyPropertyChanged
        ''' <summary>
        ''' returns true if the Entry is mapped to a class member field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property IsMapped As Boolean

        ''' <summary>
        ''' True if ObjectEntry has a defined lower value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasLowerRangeValue() As Boolean

        ''' <summary>
        ''' gets the lower range Value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property LowerRangeValue() As Long?

        ''' <summary>
        ''' True if ObjectEntry has a defined upper value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasUpperRangeValue() As Boolean

        ''' <summary>
        ''' gets the upper range Value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property UpperRangeValue() As Long?

        ''' <summary>
        ''' gets the list of possible values
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasPossibleValues() As Boolean

        ''' <summary>
        ''' gets the list of possible values
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property PossibleValues() As List(Of String)

        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Property Description() As String

        ''' <summary>
        ''' sets or gets the object name of the entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Objectname() As String

        ''' <summary>
        ''' sets or gets the XchangeManager ID for the field 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property XID() As String

        ''' <summary>
        ''' returns the name of the entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Entryname() As String

        ''' <summary>
        ''' sets or gets the type otObjectEntryDefinitionType
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Typeid() As otObjectEntryType

        ''' <summary>
        ''' sets or gets true if this field is a spare field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property IsSpareField() As Boolean

        '''' <summary>
        '''' returns the field data type
        '''' </summary>
        '''' <value></value>
        '''' <returns></returns>
        '''' <remarks></remarks>
        Property Datatype() As otDataType
        ''' <summary>
        ''' returns version
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Version() As Long

        ''' <summary>
        ''' returns a array of aliases
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Aliases() As String()

        ''' <summary>
        ''' returns Title (Column Header)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Title() As String

        ''' <summary>
        ''' sets or gets the default value for the object entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property DefaultValue As Object

        ''' <summary>
        ''' returns True if the Entry is a Column
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property IsColumn As Boolean

        ''' <summary>
        ''' returns true if the Entry is a Compound entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property IsCompound As Boolean

        ''' <summary>
        ''' sets or gets the condition for dynamically looking up values
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property LookupCondition As String

        ''' <summary>
        ''' returns true if there is a dynamically lookup condition
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasLookupCondition As Boolean

        ReadOnly Property HasValidationProperties As Boolean

        Property ValidationProperties As List(Of ObjectValidationProperty)

        ReadOnly Property HasValidateRegExpression As Boolean

        Property ValidateRegExpression As String

        Property IsValidating As Boolean

        ReadOnly Property HasRenderProperties As Boolean

        Property RenderProperties As List(Of RenderProperty)

        ReadOnly Property HasRenderRegExpression As Boolean

        Property RenderRegExpMatch As String

        Property RenderRegExpPattern As String

        Property IsRendering As Boolean

        Property Properties As List(Of ObjectEntryProperty)

        Property Size As Long?

        Property IsNullable As Boolean

        Property PrimaryKeyOrdinal As Long

        Property InnerDatatype As otDataType?

        Property Ordinal As Long

        Property IsReadonly As Boolean

        Property IsActive As Boolean

        Property LookupProperties As List(Of LookupProperty)

        ReadOnly Property HasLookupProperties As Boolean

        Property LookupPropertyStrings As String()

        Property ValidationPropertyStrings As String()

        Property RenderPropertyStrings As String()

        Property PropertyStrings As String()

        ''' <summary>
        ''' gets or sets the category
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Category As String

        ''' <summary>
        ''' set the object entry by the attribute
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function SetByAttribute(attribute As ormObjectEntryAttribute) As Boolean

        ''' <summary>
        ''' handler for the OnSwitchRuntimeOff event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnswitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs)

        Function GetObjectDefinition() As ObjectDefinition


    End Interface

    ''' <summary>
    ''' interface for a enumeration of data objects or ormResluts against the database
    ''' </summary>
    ''' <remarks>
    ''' design principles
    ''' 1. offer an interface independent on the query language for enumerating data objects or getting result by orm Record
    ''' </remarks>
    Public Interface iormQueriedEnumeration
        Inherits IEnumerable(Of iormRelationalPersistable)

        ''' <summary>
        ''' Event OnLoading is raised when the query execution is started
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Event OnLoading(sender As Object, e As System.EventArgs)

        ''' <summary>
        ''' Event OnLoaded is raised when the query execution has ended
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Event OnLoaded(sender As Object, e As System.EventArgs)

        ''' <summary>
        ''' Event OnAdding is raised when the query result set is extended
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Event OnAdded(sender As Object, e As System.EventArgs)
        ''' <summary>
        ''' Event OnRemoving is raised when the query result set is reduced
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Event OnRemoved(sender As Object, e As System.EventArgs)

        ''' <summary>
        ''' true if the query has run and a result is loaded
        ''' </summary>
        ''' <value></value>
        ReadOnly Property IsLoaded As Boolean

        ''' <summary>
        ''' load the query result by running the query against the database
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Load(Optional domainid As String = Nothing) As Boolean

        ''' <summary>
        ''' returns the primary Object Definition
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetObjectDefinition() As ObjectDefinition
        ''' <summary>
        ''' remove the data object at position in the query result
        ''' </summary>
        ''' <param name="no"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function RemoveObject(no As ULong) As Boolean
        ''' <summary>
        ''' adds a database object to the results of the query
        ''' </summary>
        ''' <param name="no"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function AddObject(dataobject As iormRelationalPersistable, Optional ByRef no As ULong? = Nothing) As Boolean
        ''' <summary>
        ''' returns the primary ClassDescription
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetObjectClassDescription() As ObjectClassDescription
        ''' <summary>
        ''' Gets the id of this queried enumeration.
        ''' </summary>
        ''' <value>The id.</value>
        ReadOnly Property ID As String
        ''' <summary>
        ''' gets or sets all object entry names of the query
        ''' </summary>
        ''' <param name="ordered"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ObjectEntryNames As IList(Of String)
        ''' <summary>
        ''' Gets or sets the is objects enumerated flag - true if objects are going to be returned otherwise ormRecord could be returned
        ''' </summary>
        ''' <value>The is object enumerated.</value>
        Property AreObjectsEnumerated As Object

        ''' <summary>
        ''' returns a list of iormObjectEntry by name  returned by this Queried Enumeration
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetObjectEntry(name As String) As iormObjectEntry

        ''' <summary>
        ''' returns a list of iormObjectEntry entries returned by this Queried Enumeration
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetObjectEntries() As IList(Of iormObjectEntry)

        ''' <summary>
        ''' resets the result but not the query itself
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Reset() As Boolean
        ''' <summary>
        ''' returns the zero-based ormRecord of the qry result
        ''' </summary>
        ''' <param name="no"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetRecord(no As ULong) As ormRecord
        ''' <summary>
        ''' returns an infused object out of the zero-based number or results
        ''' </summary>
        ''' <param name="no"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetObject(no As ULong) As iormRelationalPersistable
        ''' <summary>
        ''' returns the size of the result list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Count As ULong

        ''' <summary>
        ''' gets the value of a query parameter
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetValue(name As String, ByRef value As Object) As Boolean

        ''' <summary>
        ''' sets the value of query parameter
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function SetValue(name As String, value As Object) As Boolean

    End Interface
End Namespace
