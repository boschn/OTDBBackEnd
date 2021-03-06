﻿Option Explicit On

REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** CLASS Repository for On Track Database Backend Library
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2014-01-08
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************''' <summary>

Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports System.Diagnostics
Imports System.Diagnostics.Debug
Imports System.Text.RegularExpressions
Imports System.Collections.Concurrent

Imports System.IO
Imports System.Threading

Imports OnTrack
Imports OnTrack.Database
Imports System.Reflection
Imports System.Reflection.Emit
Imports System.Linq.Expressions

Namespace OnTrack


    ''' <summary>
    ''' store for attribute information in the dataobject classes - relies in the CORE
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ObjectClassRepository

        '*** Event Arguments
        Public Class EventArgs
            Inherits System.EventArgs

            Private _id As String
            Private _description As ObjectClassDescription

            Public Sub New(objectname As String, description As ObjectClassDescription)
                _id = objectname
                _description = description
            End Sub

            ''' <summary>
            ''' Gets the object class description.
            ''' </summary>
            ''' <value>The objectdefinition.</value>
            Public ReadOnly Property Description() As ObjectClassDescription
                Get
                    Return Me._description
                End Get
            End Property

            ''' <summary>
            ''' Gets the objectname.
            ''' </summary>
            ''' <value>The objectname.</value>
            Public ReadOnly Property Objectname() As String
                Get
                    Return Me._id
                End Get
            End Property

        End Class

        Private _isInitialized As Boolean = False
        Private _lock As New Object
        Private _BootStrapSchemaCheckSum As ULong

        '** stores
        Private _CreateInstanceDelegateStore As New Dictionary(Of String, ObjectClassDescription.CreateInstanceDelegate) ' Class Name and Delegate for Instance Creator
        Private _DescriptionsByClassTypeDescriptionStore As New Dictionary(Of String, ObjectClassDescription) 'name of classes with id
        Private _DescriptionsByIDDescriptionStore As New Dictionary(Of String, ObjectClassDescription) 'name of classes with id
        Private _Table2ObjectClassStore As New Dictionary(Of String, List(Of Type)) 'name of tables to types
        Private _BootstrapObjectClasses As New List(Of Type)
        Private _ClassDescriptorPerModule As New Dictionary(Of String, List(Of ObjectClassDescription))
        Private _TableAttributesStore As New Dictionary(Of String, ormSchemaTableAttribute)

        Public Event OnObjectClassDescriptionLoaded(sender As Object, e As ObjectClassRepository.EventArgs)

        ''' <summary>
        ''' constructor of the object class repository
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()

        End Sub

#Region "Properties"


        ''' <summary>
        ''' returns the count for the class description store (all classes in store)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Count As ULong
            Get
                Me.Initialize()
                Return _DescriptionsByClassTypeDescriptionStore.Count
            End Get
        End Property
        ''' <summary>
        ''' returns an IEnumerable of all ObjectClassDescriptions
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectClassDescriptions As IEnumerable(Of ObjectClassDescription)
            Get
                Me.Initialize()
                Return _DescriptionsByClassTypeDescriptionStore.Values
            End Get
        End Property
        ''' <summary>
        ''' gets the Checksum of the ObjectClassRepository for Bootstrapping classes 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Property BootstrapSchemaChecksum As ULong
            Get
                Return _BootStrapSchemaCheckSum
            End Get
            Private Set(value As ULong)
                _BootStrapSchemaCheckSum = value
            End Set
        End Property
#End Region

        ''' <summary>
        ''' Add oder modify a table attribute 
        ''' </summary>
        ''' <param name="tableattribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AlterTableAttribute(ByRef tableattribute As ormSchemaTableAttribute, Optional fieldinfo As FieldInfo = Nothing) As Boolean
            Dim aTableattribute As ormSchemaTableAttribute
            Dim afieldvalue As String
            Dim aTablename As String

            If fieldinfo IsNot Nothing Then
                afieldvalue = fieldinfo.GetValue(Nothing).ToString.ToUpper
            End If

            '***
            If tableattribute.HasValueTableName Then
                aTablename = tableattribute.TableName
            ElseIf fieldinfo IsNot Nothing Then
                aTablename = afieldvalue
            ElseIf tableattribute.HasValueID Then
                aTablename = tableattribute.ID
            Else
                CoreMessageHandler(message:="cannot determine tablename", subname:="ObjectClassrepository.AlterTableAttribute", _
                                   messagetype:=otCoreMessageType.InternalError)
                Return False
            End If

            If _TableAttributesStore.ContainsKey(aTablename) Then
                aTableattribute = _TableAttributesStore.Item(aTablename)
                '** default values
                With aTableattribute
                    '**
                    If Not .HasValueTableName Then .TableName = aTablename
                    '** version
                    If tableattribute.HasValueVersion Then
                        If Not .HasValueVersion Then
                            .Version = tableattribute.Version
                        ElseIf .Version < tableattribute.Version Then
                            .Version = tableattribute.Version
                        End If
                    End If

                    '** copy
                    '** true overrules
                    If (.HasValueAddDomainBehavior AndAlso Not .AddDomainBehavior AndAlso tableattribute.HasValueAddDomainBehavior) _
                        OrElse (Not .HasValueAddDomainBehavior AndAlso tableattribute.HasValueAddDomainBehavior) Then
                        .AddDomainBehavior = tableattribute.AddDomainBehavior
                    End If
                    If (.HasValueDeleteFieldBehavior AndAlso Not .AddDeleteFieldBehavior AndAlso tableattribute.HasValueDeleteFieldBehavior) _
                       OrElse (Not .HasValueDeleteFieldBehavior AndAlso tableattribute.HasValueDeleteFieldBehavior) Then
                        .AddDeleteFieldBehavior = tableattribute.AddDeleteFieldBehavior
                    End If
                    If (.HasValueSpareFields AndAlso Not .HasValueSpareFields AndAlso tableattribute.HasValueSpareFields) _
                      OrElse (Not .HasValueSpareFields AndAlso tableattribute.HasValueSpareFields) Then
                        .AddSpareFields = tableattribute.AddSpareFields
                    End If
                    If (.HasValueUseCache AndAlso Not .UseCache AndAlso tableattribute.HasValueUseCache) _
                     OrElse (Not .HasValueUseCache AndAlso tableattribute.HasValueUseCache) Then
                        .UseCache = tableattribute.UseCache
                    End If
                    '** other
                    If Not .HasValueDescription AndAlso tableattribute.HasValueDescription Then
                        .Description = tableattribute.Description
                    End If
                    If Not .HasValuePrimaryKey AndAlso tableattribute.HasValuePrimaryKey Then
                        .PrimaryKey = tableattribute.PrimaryKey
                    End If
                    If Not .HasValueID AndAlso tableattribute.HasValueID Then
                        .ID = tableattribute.ID
                    End If

                    '** import foreign keys
                    For Each afk In tableattribute.ForeignKeyAttributes
                        If Not .HasForeignkey(afk.ID) Then
                            .AddForeignKey(afk)
                        End If
                    Next
                    '** import columns
                    For Each acol In tableattribute.ColumnAttributes
                        If Not .HasColumn(acol.ColumnName) Then
                            .AddColumn(acol)
                        End If
                    Next

                End With
                '** overwrite
                tableattribute = aTableattribute
            Else
                '** take the new one
                With tableattribute
                    '**
                    .TableName = aTablename
                    '** version
                    If Not .HasValueVersion Then .Version = 1
                End With
                _TableAttributesStore.Add(key:=tableattribute.TableName.ToUpper, value:=tableattribute)
            End If

        End Function
        ''' <summary>
        ''' returns the names of the bootstrapping tables
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetBootStrapObjectClassDescriptions() As List(Of ObjectClassDescription)
            Me.Initialize()
            Dim aList = New List(Of ObjectClassDescription)
            For Each aClasstype In _BootstrapObjectClasses
                Dim anObjectDescription As ObjectClassDescription = Me.GetObjectClassDescription(aClasstype)
                If anObjectDescription IsNot Nothing Then
                    If Not aList.Contains(anObjectDescription) Then aList.Add(anObjectDescription)
                Else
                    CoreMessageHandler(message:="Object Description not found for bootstrapping classes", objectname:=aClasstype.Name, _
                                       subname:="objectClassRepository.GetBootStrapObjectClassDescriptions", messagetype:=otCoreMessageType.InternalError)
                End If
            Next
            Return aList
        End Function
        ''' <summary>
        ''' returns the names of the bootstrapping tables
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetBootStrapTableNames() As List(Of String)
            Me.Initialize()
            Dim aList = New List(Of String)
            For Each aClasstype In _BootstrapObjectClasses
                Dim anObjectDescription As ObjectClassDescription = Me.GetObjectClassDescription(aClasstype)
                If anObjectDescription IsNot Nothing Then
                    For Each aName In anObjectDescription.Tables
                        If Not aList.Contains(aName.ToUpper) Then aList.Add(aName.ToUpper)
                    Next
                Else
                    CoreMessageHandler(message:="Object Description not found for bootstrapping classes", objectname:=aClasstype.Name, _
                                       subname:="objectClassRepository.getBootStrapTablesNames", messagetype:=otCoreMessageType.InternalError)
                End If
            Next
            Return aList
        End Function
        ''' <summary>
        ''' returns the ObjectClass Type for an object class name
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateInstance(type As System.Type) As iormPersistable
            Try
                If Not _CreateInstanceDelegateStore.ContainsKey(key:=Type.FullName.ToUpper) Then
                    CoreMessageHandler(message:="type is not found in the instance creator store of class descriptions", _
                                       arg1:=Type.FullName, subname:="ObjectClassRepository.CreateInstance", messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If

                Dim aDelegate As ObjectClassDescription.CreateInstanceDelegate = _CreateInstanceDelegateStore.Item(key:=type.FullName.ToUpper)
                Dim anObject As iormPersistable = aDelegate()
                Return anObject
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectClassRepository.CreateInstance", arg1:=Type.FullName)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' returns the ObjectClass Type for an object class name
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassType(objectname As String) As System.Type
            If _DescriptionsByIDDescriptionStore.ContainsKey(key:=objectname.ToUpper) Then
                Return _DescriptionsByIDDescriptionStore.Item(key:=objectname.ToUpper).Type
            End If
            Return Nothing
        End Function

        ''' <summary>
        ''' returns the ObjectClass Description
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassDescription([type] As Type) As ObjectClassDescription
            Return GetObjectClassDescription([type].FullName)
        End Function
        ''' <summary>
        ''' returns the ObjectClassDescription for a ObjectDescription Class by name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassDescription(typename As String) As ObjectClassDescription
            Me.Initialize()

            If _DescriptionsByClassTypeDescriptionStore.ContainsKey(key:=typename) Then
                Return _DescriptionsByClassTypeDescriptionStore.Item(key:=typename)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns the ObjectClassDescription for a ObjectDescription Class by name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassDescriptionByID(id As String) As ObjectClassDescription
            Me.Initialize()

            If _DescriptionsByIDDescriptionStore.ContainsKey(key:=id.ToUpper) Then
                Return _DescriptionsByIDDescriptionStore.Item(key:=id.ToUpper)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns the SchemaTableAttribute for a table name
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetTableAttribute(tablename As String) As ormSchemaTableAttribute
            Me.Initialize()

            If _TableAttributesStore.ContainsKey(key:=tablename.ToUpper) Then
                Return _TableAttributesStore(key:=tablename.ToUpper)
            Else
                Return Nothing
            End If

        End Function

        ''' <summary>
        ''' gets a schemaColumnAttribute for tablename and columnname
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectEntryAttribute(entryname As String, objectname As String) As ormObjectEntryAttribute
            Me.Initialize()
            If _DescriptionsByIDDescriptionStore.ContainsKey(key:=objectname.ToUpper) Then
                Return _DescriptionsByIDDescriptionStore.Item(key:=objectname.ToUpper).GetObjectEntryAttribute(entryname:=entryname)
            Else
                Return Nothing
            End If

        End Function

        ''' <summary>
        ''' substitute referenced properties in the reference
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SubstituteReferencedTableColumn(ByRef attribute As ormSchemaTableColumnAttribute) As Boolean
            '*** REFERENCE OBJECT ENTRY
            If attribute.HasValueReferenceObjectEntry Then
                Dim refObjectName As String = ""
                Dim refObjectEntry As String = ""
                Dim names = Shuffle.NameSplitter(attribute.ReferenceObjectEntry)
                If names.Count > 1 Then
                    refObjectName = names(0)
                    refObjectEntry = names(1)
                Else
                    CoreMessageHandler(message:="objectname is missing in reference " & attribute.ReferenceObjectEntry, subname:="ObjectClassRepository.GetReferenceTableColumn", _
                                       messagetype:=otCoreMessageType.InternalError, arg1:=attribute.ReferenceObjectEntry, columnname:=attribute.ColumnName, tablename:=attribute.Tablename)
                    Return False
                End If

                ' will not take 
                Dim anReferenceAttribute As ormObjectEntryAttribute = _
                    Me.GetObjectEntryAttribute(entryname:=refObjectEntry, objectname:=refObjectName)

                If anReferenceAttribute IsNot Nothing Then
                    With anReferenceAttribute
                        If .HasValueID And Not attribute.HasValueID Then attribute.ID = .ID '-> should be set by the const value
                        If .HasValueTableName And Not attribute.HasValueTableName Then attribute.Tablename = .Tablename
                        If .HasValueColumnName And Not attribute.HasValueColumnName Then attribute.ColumnName = .ColumnName
                        If .HasValueRelation And Not attribute.HasValueRelation Then attribute.Relation = .Relation
                        If .HasValueIsNullable And Not attribute.HasValueIsNullable Then attribute.IsNullable = .IsNullable
                        If .HasValueIsUnique And Not attribute.HasValueIsUnique Then attribute.IsUnique = .IsUnique
                        If .HasValueDataType And Not attribute.HasValueDataType Then attribute.DataType = .DataType
                        If .HasValueInnerDataType And Not attribute.HasValueInnerDataType Then attribute.InnerDataType = .InnerDataType
                        If .HasValueSize And Not attribute.HasValueSize Then attribute.Size = .Size
                        If .HasValueDescription And Not attribute.HasValueDescription Then attribute.Description = .Description
                        If .HasValueDBDefaultValue And Not attribute.HasValueDBDefaultValue Then attribute.DBDefaultValue = .DBDefaultValue
                        If .HasValueVersion And Not attribute.HasValueVersion Then attribute.Version = .Version

                        If .HasValueUseForeignKey And Not attribute.HasValueUseForeignKey Then attribute.UseForeignKey = .UseForeignKey
                        If .HasValueForeignKeyReferences And Not attribute.HasValueForeignKeyReferences Then attribute.ForeignKeyReferences = .ForeignKeyReferences
                        If .HasValueForeignKeyProperties And Not attribute.HasValueForeignKeyProperties Then attribute.ForeignKeyProperties = .ForeignKeyProperties
                    End With

                Else
                    CoreMessageHandler(message:="referenceObjectEntry  object id '" & refObjectName & "' and column name '" & refObjectEntry & "' not found for column schema", _
                                       columnname:=attribute.ColumnName, tablename:=attribute.Tablename, subname:="ObjectClassRepository.GetReferenceTableColumn", messagetype:=otCoreMessageType.InternalError)
                End If
                Return True
            Else
                Return False
            End If

        End Function
        ''' <summary>
        ''' substitute referenced properties in the reference
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SubstituteReferencedObjectEntry(ByRef attribute As ormObjectEntryAttribute) As Boolean
            '*** REFERENCE OBJECT ENTRY
            If attribute.HasValueReferenceObjectEntry Then
                Dim refObjectName As String = ""
                Dim refObjectEntry As String = ""
                Dim names = Shuffle.NameSplitter(attribute.ReferenceObjectEntry)
                If names.Count > 1 Then
                    refObjectName = names(0)
                    refObjectEntry = names(1)
                Else
                    refObjectEntry = attribute.ReferenceObjectEntry
                    refObjectName = attribute.ObjectName
                End If

                ' will not take 
                Dim anReferenceAttribute As ormObjectEntryAttribute = _
                    Me.GetObjectEntryAttribute(entryname:=refObjectEntry, objectname:=refObjectName)

                If anReferenceAttribute IsNot Nothing Then
                    With anReferenceAttribute
                        '** read table column elements and then the object references
                        If SubstituteReferencedTableColumn(attribute:=attribute) Then
                            If .HasValueEntryType And Not attribute.HasValueEntryType Then attribute.EntryType = .EntryType
                            If .HasValueTitle And Not attribute.HasValueTitle Then attribute.Title = .Title
                            If .HasValueDescription And Not attribute.HasValueDescription Then attribute.Description = .Description

                            If .HasValueXID And Not attribute.HasValueXID Then attribute.XID = .XID
                            If .HasValueAliases And Not attribute.HasValueAliases Then attribute.Aliases = .Aliases
                            If .HasValueObjectEntryProperties And Not attribute.HasValueObjectEntryProperties Then attribute.Properties = .Properties
                            If .HasValueVersion And Not attribute.HasValueVersion Then attribute.Version = .Version
                            If .HasValueSpareFieldTag And Not attribute.HasValueSpareFieldTag Then attribute.SpareFieldTag = .SpareFieldTag

                            If .HasValueRender And Not attribute.HasValueRender Then attribute.Render = .Render
                            If .HasValueRenderProperties And Not attribute.HasValueRenderProperties Then attribute.RenderProperties = .RenderProperties
                            If .HasValueRenderRegExpMatch And Not attribute.HasValueRenderRegExpMatch Then attribute.RenderRegExpMatch = .RenderRegExpMatch
                            If .HasValueRenderRegExpPattern And Not attribute.HasValueRenderRegExpPattern Then attribute.RenderRegExpPattern = .RenderRegExpPattern

                            If .HasValueValidate And Not attribute.HasValueValidate Then attribute.Validate = .Validate
                            If .HasValueLowerRange And Not attribute.HasValueLowerRange Then attribute.LowerRange = .LowerRange
                            If .HasValueUpperRange And Not attribute.HasValueUpperRange Then attribute.UpperRange = .UpperRange
                            If .HasValueValidationProperties And Not attribute.HasValueValidationProperties Then attribute.ValidationProperties = .ValidationProperties
                            If .HasValueLookupCondition And Not attribute.HasValueLookupCondition Then attribute.LookupCondition = .LookupCondition
                            If .HasValueValues And Not attribute.HasValueValues Then attribute.Values = .Values
                        End If

                    End With

                Else
                    CoreMessageHandler(message:="referenceObjectEntry  object id '" & refObjectName & "' and column name '" & refObjectEntry & "' not found for column schema", _
                                       entryname:=attribute.EntryName, objectname:=attribute.ObjectName, subname:="ObjectClassRepository.SubstituteReferencedObjectEntry", messagetype:=otCoreMessageType.InternalError)
                End If
                Return True
            Else
                Return False
            End If

        End Function
        ''' <summary>
        ''' returns the schemaColumnAttribute for a given columnname and tablename
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSchemaColumnAttribute(columnname As String, Optional tablename As String = "") As ormSchemaTableColumnAttribute
            Me.Initialize()
            Dim aFieldname As String = ""
            Dim aTablename As String = ""
            Dim names() As String = Shuffle.NameSplitter(columnname)
            Dim anAttribute As ormSchemaTableColumnAttribute



            '** split the names
            If tablename <> "" And names.Count = 1 Then
                aFieldname = columnname.ToUpper
                aTablename = tablename.ToUpper
            ElseIf names.Count > 1 AndAlso tablename = "" Then
                aTablename = names(0)
                aFieldname = names(1)
            Else
                CoreMessageHandler(message:="more than one tables in the description but no table name specified in the column name or as argument", _
                                   messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.getSchemaColumnAttribute", _
                                   arg1:=names, tablename:=tablename, columnname:=columnname)
                Return Nothing
            End If

            '** return
            If _TableAttributesStore.ContainsKey(key:=tablename.ToUpper) Then
                anAttribute = _TableAttributesStore.Item(key:=aTablename).GetColumn(aFieldname)
                '*** substitute references
                SubstituteReferencedTableColumn(attribute:=anAttribute)
                '** return
                Return anAttribute

            Else
                Return Nothing
            End If

        End Function
        ''' <summary>
        ''' gets a list of ObjectClassDescriptions per tablename or empty if none
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassDescriptionsByTable(tablename As String, Optional onlyenabled As Boolean = True) As List(Of ObjectClassDescription)
            Me.Initialize()
            Dim alist As New List(Of ObjectClassDescription)
            If Not _TableAttributesStore.ContainsKey(tablename.ToUpper) Then Return alist
            If onlyenabled AndAlso Not _TableAttributesStore.Item(tablename.ToUpper).Enabled Then Return alist

            If _Table2ObjectClassStore.ContainsKey(tablename.ToUpper) Then
                For Each aObjectType In _Table2ObjectClassStore.Item(tablename.ToUpper)
                    alist.Add(GetObjectClassDescription(aObjectType))
                Next
            End If
            Return alist
        End Function
        ''' <summary>
        ''' returns a list of ObjectClassDescriptions per module name
        ''' </summary>
        ''' <param name="modulename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassDescriptions(modulename As String) As List(Of ObjectClassDescription)
            Me.Initialize()
            If _ClassDescriptorPerModule.ContainsKey(key:=modulename.ToUpper) Then
                Return _ClassDescriptorPerModule.Item(key:=modulename.ToUpper)
            Else
                Return New List(Of ObjectClassDescription)
            End If
        End Function

        ''' <summary>
        ''' returns a list of all Modulenames
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetModulenames() As List(Of String)
            Me.Initialize()
            Return _ClassDescriptorPerModule.Keys.ToList
        End Function
        ''' <summary>
        ''' gets a list of object classes which are using a tablename for persistence
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassesForTable(tablename As String, Optional onlyenabled As Boolean = True) As List(Of Type)
            Me.Initialize()
            If Not _TableAttributesStore.ContainsKey(tablename.ToUpper) Then Return New List(Of Type)
            If onlyenabled AndAlso Not _TableAttributesStore.Item(tablename.ToUpper).Enabled Then Return New List(Of Type)

            If _Table2ObjectClassStore.ContainsKey(key:=tablename.ToUpper) Then
                Return _Table2ObjectClassStore.Item(key:=tablename.ToUpper)
            Else
                Return New List(Of Type)
            End If
        End Function

        ''' <summary>
        ''' register a CacheManager at the ObjectClassRepository
        ''' </summary>
        ''' <param name="cache"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RegisterCacheManager(cache As iormObjectCacheManager) As Boolean
            AddHandler OnObjectClassDescriptionLoaded, AddressOf cache.OnObjectClassDescriptionLoaded
        End Function
        ''' <summary>
        ''' Initialize the Repository
        ''' </summary>
        ''' <param name="force"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Initialize(Optional force As Boolean = False) As Boolean

            If IsInitialized Or Not force Then Return True
            Dim aFieldList As System.Reflection.FieldInfo()

            '*** select all the dataobjects
            ''' register all data objects which have a direct orm mapping
            ''' implementation of the interface iormpersistable

            Dim thisAsm As Assembly = Assembly.GetExecutingAssembly()
            Dim adataObjectClassLists As List(Of Type) = thisAsm.GetTypes().Where(Function(t) _
                                                                                  ((GetType(iormPersistable).IsAssignableFrom(t) AndAlso t.IsClass AndAlso Not t.IsAbstract))).ToList()
            _BootStrapSchemaCheckSum = 0

            '*** go through the classes in the assembly
            For Each aClass In adataObjectClassLists
                '* add it to _classes
                If _DescriptionsByClassTypeDescriptionStore.ContainsKey(aClass.FullName) Then _DescriptionsByClassTypeDescriptionStore.Remove(key:=aClass.FullName)
                Dim anewDescription As New ObjectClassDescription(aClass, Me)


                ''' check the class type attributes
                '''
                For Each anAttribute As System.Attribute In aClass.GetCustomAttributes(False)
                    ''' Object Attribute
                    ''' 
                    If anAttribute.GetType().Equals(GetType(ormObjectAttribute)) Then
                        Dim anObjectAttribute = DirectCast(anAttribute, ormObjectAttribute)
                        '** bootstrapping classes ??
                        If anObjectAttribute.HasValueIsBootstap Then
                            If anObjectAttribute.IsBootstrap Then
                                If Not _BootstrapObjectClasses.Contains(aClass) Then
                                    _BootstrapObjectClasses.Add(aClass)
                                End If
                            End If

                        Else
                            anObjectAttribute.IsBootstrap = False ' default
                        End If
                        ''' remove ObjectID
                        If _DescriptionsByIDDescriptionStore.ContainsKey(key:=anObjectAttribute.ID) Then
                            _DescriptionsByIDDescriptionStore.Remove(key:=anObjectAttribute.ID)
                        End If
                        ''' Add both
                        _DescriptionsByIDDescriptionStore.Add(key:=anObjectAttribute.ID, value:=anewDescription)
                        _DescriptionsByClassTypeDescriptionStore.Add(key:=aClass.FullName, value:=anewDescription)
                    End If
                Next

                ''' create the InstanceCreator
                ''' 
                'Dim func As Type = GetType(Func(Of ))
                'Dim delegatetype As Type = func.MakeGenericType()
                Dim aCreator As ObjectClassDescription.CreateInstanceDelegate = _
                    ObjectClassDescription.CreateILGCreateInstanceDelegate(aClass.GetConstructor(Type.EmptyTypes), GetType(ObjectClassDescription.CreateInstanceDelegate))
                If _CreateInstanceDelegateStore.ContainsKey(key:=aClass.FullName.ToUpper) Then
                    _CreateInstanceDelegateStore.Remove(key:=aClass.FullName.ToUpper)
                End If
                _CreateInstanceDelegateStore.Add(key:=aClass.FullName.ToUpper, value:=aCreator)

                ''' get the Fieldlist especially collect the constants
                ''' 

                aFieldList = aClass.GetFields(Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Public Or _
                                              Reflection.BindingFlags.Static Or BindingFlags.FlattenHierarchy)

                '** look into each Const Type (Fields)
                For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList
                    If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                        '** Attributes
                        For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                            ''' check for tables first to get them all before we process the
                            ''' objects in details
                            If anAttribute.GetType().Equals(GetType(ormSchemaTableAttribute)) Then
                                Dim alist As List(Of Type)

                                ''' do we have the same const variable name herited from other classes ?
                                ''' take then only the local / const variable with attributes from the herited class (overwriting)

                                Dim localfield As FieldInfo = aClass.GetField(name:=aFieldInfo.Name, bindingAttr:=Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Public Or _
                                              Reflection.BindingFlags.Static)
                                If localfield Is Nothing OrElse (localfield IsNot Nothing AndAlso aFieldInfo.DeclaringType.Equals(localfield.ReflectedType)) Then


                                    '** Type Definition
                                    If _Table2ObjectClassStore.ContainsKey(aFieldInfo.GetValue(Nothing).ToString.ToUpper) Then
                                        alist = _Table2ObjectClassStore.Item(aFieldInfo.GetValue(Nothing).ToString.ToUpper)
                                    Else
                                        alist = New List(Of Type)
                                        _Table2ObjectClassStore.Add(key:=aFieldInfo.GetValue(Nothing).ToString.ToUpper, value:=alist)
                                    End If
                                    If Not alist.Contains(item:=aClass) Then
                                        alist.Add(aClass)
                                    End If

                                    '*** Calculate the Checksum from the Tableversions in the Bootstrapclasses
                                    If _BootstrapObjectClasses.Contains(aClass) Then
                                        If Not DirectCast(anAttribute, ormSchemaTableAttribute).HasValueVersion Then
                                            DirectCast(anAttribute, ormSchemaTableAttribute).Version = 1
                                        End If
                                        Dim i = _BootstrapObjectClasses.IndexOf(aClass)
                                        _BootStrapSchemaCheckSum += DirectCast(anAttribute, ormSchemaTableAttribute).Version * Math.Pow(10, i)
                                    End If

                                    '*** add to global tableattribute store
                                    Me.AlterTableAttribute(anAttribute, fieldinfo:=aFieldInfo)
                                End If

                                '*** Object Attribute
                                ''' check for Object Attributes bound to constants in the class
                                ''' 
                            ElseIf anAttribute.GetType().Equals(GetType(ormObjectAttribute)) Then
                                Dim anObjectAttribute = DirectCast(anAttribute, ormObjectAttribute)
                                If anObjectAttribute.HasValueIsBootstap Then
                                    If anObjectAttribute.IsBootstrap Then
                                        If Not _BootstrapObjectClasses.Contains(aClass) Then
                                            _BootstrapObjectClasses.Add(aClass)
                                        End If
                                    End If
                                Else
                                    anObjectAttribute.IsBootstrap = False ' default
                                End If
                                ''' remove ObjectID
                                If _DescriptionsByIDDescriptionStore.ContainsKey(key:=anObjectAttribute.ID) Then
                                    _DescriptionsByIDDescriptionStore.Remove(key:=anObjectAttribute.ID)
                                End If
                                ''' Add both
                                _DescriptionsByIDDescriptionStore.Add(key:=anObjectAttribute.ID, value:=anewDescription)
                                _DescriptionsByClassTypeDescriptionStore.Add(key:=aClass.FullName, value:=anewDescription)
                            End If

                        Next
                    End If
                Next
            Next

            '***
            '*** go through all classes
            '*** and get the attributes to look into
            '*** 

            Try
                For Each aClassDescription In _DescriptionsByClassTypeDescriptionStore.Values.ToList
                    If aClassDescription.Initialize() Then
                        '*** sort per module
                        If aClassDescription.ObjectAttribute.HasValueModulename Then
                            Dim aName As String = aClassDescription.ObjectAttribute.Modulename.ToUpper
                            Dim aList = New List(Of ObjectClassDescription)
                            If Not _ClassDescriptorPerModule.ContainsKey(key:=aName) Then
                                _ClassDescriptorPerModule.Add(key:=aName, value:=aList)
                            Else
                                aList = _ClassDescriptorPerModule.Item(key:=aName)
                            End If
                            aList.Add(aClassDescription)
                        End If
                    Else
                        '** remove from store if initialiazing failed
                        _DescriptionsByClassTypeDescriptionStore.Remove(key:=aClassDescription.Name.ToUpper)
                    End If
                Next

                _isInitialized = True
                Return True
            Catch ex As Exception

                Call CoreMessageHandler(subname:="ObjectClassRepository.Initialize", exception:=ex)

            End Try

        End Function
    End Class

    ''' <summary>
    '''  class to hold per Class the orM Attributes and FieldInfo for Mapping and Relation
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ObjectClassDescription

        Public Const ConstMTRetrieve = "RETRIEVE"
        Public Const ConstMTCreateDataObject = "CREATEDATAOBJECT"

        ''' <summary>
        ''' Delegates
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Delegate Function CreateInstanceDelegate() As iormPersistable
        Public Delegate Function OperationCallerDelegate(dataobject As Object, parameters As Object()) As Object
        Public Delegate Function MappingGetterDelegate(dataobject As Object) As Object

        ''' <summary>
        ''' internal Store
        ''' </summary>
        ''' <remarks></remarks>
        Private _Type As Type
        Private _ObjectAttribute As ormObjectAttribute
        Private _TableAttributes As New Dictionary(Of String, ormSchemaTableAttribute) 'name of table to Attribute
        Private _ObjectEntryAttributes As New Dictionary(Of String, ormObjectEntryAttribute) 'name of object entry to Attribute
        Private _ObjectTransactionAttributes As New Dictionary(Of String, ormObjectTransactionAttribute) 'name of object entry to Attribute
        Private _ObjectOperationAttributes As New Dictionary(Of String, ormObjectOperationMethodAttribute) 'name of object entry to Attribute
        Private _ObjectOperationAttributesByTag As New Dictionary(Of String, ormObjectOperationMethodAttribute) 'name of object entry to Attribute

        Private _OperationCallerDelegates As New Dictionary(Of String, OperationCallerDelegate) ' dictionary of columns to mappings field to getter delegates
        Private _ObjectEntriesPerTable As New Dictionary(Of String, Dictionary(Of String, ormObjectEntryAttribute)) ' dictionary of tables to dictionary of columns

        Private _TableColumnsMappings As New Dictionary(Of String, Dictionary(Of String, List(Of FieldInfo))) ' dictionary of tables to dictionary of fieldmappings
        Private _ColumnEntryMapping As New Dictionary(Of String, List(Of FieldInfo)) ' dictionary of columns to mappings
        Private _MappingSetterDelegates As New Dictionary(Of String, Action(Of ormDataObject, Object)) ' dictionary of field to setter delegates
        Private _MappingGetterDelegates As New Dictionary(Of String, MappingGetterDelegate) ' dictionary of columns to mappings field to getter delegates

        Private _TableIndices As New Dictionary(Of String, Dictionary(Of String, ormSchemaIndexAttribute)) ' dictionary of tables to dictionary of indices
        Private _Indices As New Dictionary(Of String, ormSchemaIndexAttribute) ' dictionary of columns to mappings

        Private _TableRelationMappings As New Dictionary(Of String, Dictionary(Of String, List(Of FieldInfo))) ' dictionary of tables to dictionary of relation mappings
        Private _RelationEntryMapping As New Dictionary(Of String, List(Of FieldInfo)) ' dictionary of relations to mappings
        Private _TableRelations As New Dictionary(Of String, Dictionary(Of String, ormRelationAttribute)) ' dictionary of tables to dictionary of relation
        Private _Relations As New Dictionary(Of String, ormRelationAttribute) ' dictionary of relations 

        Private _DataOperationHooks As New Dictionary(Of String, RuntimeMethodHandle)
        Private _EntryMappings As New Dictionary(Of String, ormEntryMapping)

        Private _ForeignKeys As New Dictionary(Of String, Dictionary(Of String, ormSchemaForeignKeyAttribute)) 'dictionary of tables and foreign keys by ids
        Private _QueryAttributes As New Dictionary(Of String, ormObjectQueryAttribute) 'dictionary of queries and definitions

        Private _isInitalized As Boolean = False
        Private _lock As New Object

        '' caches
        Private _cachedMappedColumnnames As List(Of String) = Nothing
        Private _cachedColumnnames As List(Of String) = Nothing
        Private _cachedEntrynames As List(Of String) = Nothing
        Private _cachedQuerynames As List(Of String) = Nothing
        Private _cachedTablenames As List(Of String) = Nothing
        Private _cachedRelationNames As List(Of String) = Nothing

        '** backreference
        Private _repository As ObjectClassRepository
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="class"></param>
        ''' <remarks></remarks>
        Public Sub New([class] As Type, repository As ObjectClassRepository)
            _Type = [class]
            _repository = repository
        End Sub

        ''' <summary>
        ''' Gets or sets the object attribute.
        ''' </summary>
        ''' <value>The object attribute.</value>
        Public Property ObjectAttribute() As ormObjectAttribute
            Get
                Return Me._ObjectAttribute
            End Get
            Set(value As ormObjectAttribute)
                Me._ObjectAttribute = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the object attribute.
        ''' </summary>
        ''' <value>The object attribute.</value>
        Public ReadOnly Property PrimaryKeyEntryNames() As String()
            Get
                If _ObjectAttribute IsNot Nothing Then Return Me._ObjectAttribute.PrimaryKeys
                Return {}
            End Get

        End Property
        ''' <summary>
        ''' returns the ID of the ObjectClassDescription (the constObjectID)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ID As String
            Get
                If _ObjectAttribute IsNot Nothing Then Return _ObjectAttribute.ID
                Return Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the type.
        ''' </summary>
        ''' <value>The type.</value>
        Public Property [Type]() As Type
            Get
                Return Me._Type
            End Get
            Set(value As Type)
                Me._Type = value
            End Set
        End Property

        ''' <summary>
        ''' Name of the Class
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Name As String
            Get
                Return _Type.Name
            End Get
        End Property

        ''' <summary>
        ''' gets the primary table
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PrimaryTable As String
            Get
                Return _TableAttributes.Keys.ToList.First
            End Get
        End Property
        ''' <summary>
        ''' gets a List of all table names
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Tables As List(Of String)
            Get
                If _cachedTablenames Is Nothing Then
                    Dim theNames As New List(Of String)
                    Dim aList = _TableAttributes.Values.Where(Function(x) x.Enabled = True) ' only the enabled
                    If aList IsNot Nothing Then
                        theNames = aList.Select(Function(x) x.TableName).ToList ' get the remaining keynames
                    End If
                    _cachedTablenames = theNames
                End If
                Return _cachedTablenames
            End Get
        End Property
        ''' <summary>
        ''' gets a List of all queries
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Querynames As List(Of String)
            Get
                If _cachedQuerynames Is Nothing Then
                    Dim theNames As New List(Of String)
                    Dim aList = _QueryAttributes.Where(Function(x) x.Value.Enabled = True) ' only the enabled
                    If aList IsNot Nothing Then
                        theNames = aList.Select(Function(x) x.Key).ToList ' get the remaining keynames
                    End If
                    _cachedQuerynames = theNames
                End If
                Return _cachedQuerynames
            End Get
        End Property
        ''' <summary>
        ''' gets a List of all entry names
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Entrynames As List(Of String)
            Get
                If _cachedEntrynames Is Nothing Then
                    Dim aList As New List(Of String)
                    For Each anAttribute In _ObjectEntryAttributes.Values.Where(Function(x) x.Enabled = True)
                        If anAttribute.Enabled Then
                            If anAttribute.HasValueEntryName AndAlso Not aList.Contains(anAttribute.EntryName) Then aList.Add(anAttribute.EntryName)
                        End If
                    Next
                    _cachedEntrynames = aList
                End If
                Return _cachedEntrynames
            End Get
        End Property
        ''' <summary>
        ''' gets a List of all enabled column names
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ColumnNames As List(Of String)
            Get
                If _cachedColumnnames Is Nothing Then
                    Dim aList As New List(Of String)
                    For Each perTable In _ObjectEntriesPerTable
                        If _TableAttributes.Item(perTable.Key).Enabled Then
                            Dim entriesperTables = _ObjectEntriesPerTable.Item(key:=perTable.Key)
                            For Each anEntry In entriesperTables.Values
                                If anEntry.Enabled Then aList.Add(item:=anEntry.ColumnName)
                            Next
                        End If
                    Next
                    _cachedColumnnames = aList
                End If

                Return _cachedColumnnames
            End Get
        End Property
        ''' <summary>
        ''' gets a List of all active object transactions
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TransactionAttributes As List(Of ormObjectTransactionAttribute)
            Get
                Return _ObjectTransactionAttributes.Values.Where(Function(x) x.Enabled = True).ToList ' only the enabled
            End Get
        End Property
        ''' <summary>
        ''' gets a List of all object operations
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property OperationAttributes As List(Of ormObjectOperationMethodAttribute)
            Get
                Return _ObjectOperationAttributes.Values.ToList
            End Get
        End Property
        ''' <summary>
        ''' gets a List of all column attributes
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectEntryAttributes As List(Of ormObjectEntryAttribute)
            Get
                Dim aList As New List(Of ormObjectEntryAttribute)
                For Each anAttribute In _ObjectEntryAttributes.Values.Where(Function(x) x.Enabled = True)
                    _repository.SubstituteReferencedObjectEntry(attribute:=anAttribute)
                    SubstituteDefaultValues(attribute:=anAttribute)
                    aList.Add(anAttribute)
                Next
                Return aList
            End Get
        End Property
        ''' <summary>
        ''' gets a List of all column attributes
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MappedColumnNames As List(Of String)
            Get
                If _cachedMappedColumnnames Is Nothing Then
                    Dim aList As New List(Of String)
                    For Each aTableAttribute In _TableAttributes.Values.Where(Function(x) x.Enabled = True)
                        Dim theColumns = _TableAttributes.Item(key:=aTableAttribute.TableName).ColumnAttributes.Where(Function(x) x.Enabled = True).Select(Function(x) x.ColumnName)
                        Dim aDir As Dictionary(Of String, List(Of FieldInfo)) = _TableColumnsMappings.Item(key:=aTableAttribute.TableName)
                        For Each aColumnName In aDir.Keys
                            If theColumns.Contains(aColumnName) Then aList.Add(item:=aTableAttribute.TableName & "." & aColumnName)
                        Next
                    Next
                    _cachedMappedColumnnames = aList
                End If

                Return _cachedMappedColumnnames
            End Get
        End Property
        ''' <summary>
        ''' gets a List of all active relation names
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property RelationNames As List(Of String)
            Get
                If _cachedRelationNames Is Nothing Then
                    Dim aList As New List(Of String)
                    For Each aRelation In _Relations.Values.Where(Function(x) x.Enabled = True)
                        Dim names As String() = Shuffle.NameSplitter(aRelation.Name)
                        Dim aName As String
                        If names.Count > 1 Then
                            aName = names(1)
                        Else
                            aName = names(0)
                        End If

                        If Not aList.Contains(aName) Then aList.Add(aName)
                    Next
                    _cachedRelationNames = aList
                End If

                Return _cachedRelationNames
            End Get
        End Property
        ''' <summary>
        ''' gets a List of all index attributes
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IndexAttributes As List(Of ormSchemaIndexAttribute)
            Get
                Dim aList As New List(Of ormSchemaIndexAttribute)
                For Each aTablename In _ObjectEntriesPerTable.Keys
                    If _TableAttributes.ContainsKey(aTablename) AndAlso _TableAttributes.Item(aTablename).Enabled Then
                        Dim aList2 As List(Of ormSchemaIndexAttribute) = _TableIndices.Item(key:=aTablename).Values.Where(Function(x) x.Enabled = True).ToList
                        aList.AddRange(aList2)
                    End If
                Next
                Return aList
            End Get
        End Property
        ''' <summary>
        ''' gets a List of all relation Attributes
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property RelationAttributes As List(Of ormRelationAttribute)
            Get
                Return _Relations.Values.Where(Function(x) x.Enabled = True).ToList
            End Get
        End Property
        ''' <summary>
        ''' gets a List of all table Attributes
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TableAttributes As List(Of ormSchemaTableAttribute)
            Get
                Return _TableAttributes.Values.Where(Function(x) x.Enabled = True).ToList
            End Get
        End Property

        ''' <summary>
        ''' returns the SchemaTableAttribute for a table name
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSchemaTableAttribute(tablename As String, Optional OnlyEnabled As Boolean = True) As ormSchemaTableAttribute
            If _TableAttributes.ContainsKey(key:=tablename) Then
                Dim anAttribute As ormSchemaTableAttribute = _TableAttributes.Item(tablename)
                If OnlyEnabled Then
                    If anAttribute.Enabled Then
                        Return anAttribute
                    Else
                        Return Nothing
                    End If
                Else
                    Return anAttribute
                End If

            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns a MethodInfo for Dataoperation Hooks
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMethodInfoHook(name As String) As RuntimeMethodHandle
            If _DataOperationHooks.ContainsKey(key:=name.ToUpper) Then
                Return _DataOperationHooks.Item(key:=name.ToUpper)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' ToString Function
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ToString() As String
            Return Me.Name
        End Function

        ''' <summary>
        ''' returns the object transaction attribute 
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectTransactionAttribute(name As String, Optional onlyEnabled As Boolean = True) As ormObjectTransactionAttribute
            Dim anEntryname As String = ""
            Dim anObjectname As String = ""
            Dim names() As String = Shuffle.NameSplitter(name)

            '** split the names
            If names.Count > 1 Then
                anObjectname = names(0)
                If anObjectname <> _ObjectAttribute.ID Then
                    'CoreMessageHandler(message:="object name of Object is not equal with entry name", arg1:=anObjectname, entryname:=name, _
                    '                   subname:="ObjectClassDescription.GetObjecTransactionAttribute", messagetype:=otCoreMessageType.InternalWarning)
                    anEntryname = name.ToUpper
                    anObjectname = _ObjectAttribute.ID
                Else
                    anEntryname = names(1)
                End If

            Else
                anEntryname = name.ToUpper
            End If

            '** return

            If _ObjectTransactionAttributes.ContainsKey(key:=anEntryname) Then
                Dim anAttribute As ormObjectTransactionAttribute = _ObjectTransactionAttributes.Item(key:=anEntryname)
                If onlyEnabled Then
                    If anAttribute.Enabled Then
                        Return anAttribute
                    Else
                        Return Nothing
                    End If
                Else
                    Return anAttribute
                End If

            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' returns the object operation attribute 
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectOperationAttributeByTag(tag As String) As ormObjectOperationMethodAttribute
            '** return
            If _ObjectOperationAttributesByTag.ContainsKey(key:=tag) Then
                Dim anAttribute As ormObjectOperationMethodAttribute = _ObjectOperationAttributesByTag.Item(key:=tag)
                Return anAttribute
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' returns the object operation attribute 
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectOperationAttribute(name As String) As ormObjectOperationMethodAttribute
            Dim anEntryname As String = ""
            Dim anObjectname As String = ""
            Dim names() As String = Shuffle.NameSplitter(name)

            '** split the names
            If names.Count > 1 Then
                anObjectname = names(0)
                If anObjectname <> _ObjectAttribute.ID Then
                    'CoreMessageHandler(message:="object name of Object is not equal with entry name", arg1:=anObjectname, entryname:=name, _
                    '                   subname:="ObjectClassDescription.GetObjecTransactionAttribute", messagetype:=otCoreMessageType.InternalWarning)
                    anEntryname = name.ToUpper
                    anObjectname = _ObjectAttribute.ID
                Else
                    anEntryname = names(1)

                End If
            Else
                anEntryname = name.ToUpper
            End If

            '** return

            If _ObjectOperationAttributes.ContainsKey(key:=anEntryname) Then
                Dim anAttribute As ormObjectOperationMethodAttribute = _ObjectOperationAttributes.Item(key:=anEntryname)
                Return anAttribute
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' substitute the default values for object entry attributes
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function SubstituteDefaultValues(attribute As ormObjectEntryAttribute) As Boolean

            ''' check if we have a value otherwise take these as default
            If Not attribute.HasValueIsReadonly Then attribute.IsReadOnly = False
            If Not attribute.HasValueIsNullable Then attribute.IsNullable = False
            If Not attribute.HasValueIsUnique Then attribute.IsUnique = False
            If Not attribute.HasValueIsActive Then attribute.IsActive = True

            Return True
        End Function
        ''' <summary>
        ''' returns True if the ObjectEntry exists
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasObjectEntryAttribute(entryname As String, Optional onlyenabled As Boolean = True) As Boolean
            Dim anEntryname As String = ""
            Dim anObjectname As String = ""
            Dim names() As String = Shuffle.NameSplitter(entryname)

            '** split the names
            If names.Count > 1 Then
                anObjectname = names(0)
                If anObjectname <> _ObjectAttribute.ID Then
                    'CoreMessageHandler(message:="object name of Object is not equal with entry name", arg1:=anObjectname, entryname:=entryname, _
                    '                   subname:="ObjectClassDescription.HasObjectEntryAttribute", messagetype:=otCoreMessageType.InternalWarning)
                    anEntryname = entryname.ToUpper
                    anObjectname = _ObjectAttribute.ID
                Else
                    anEntryname = names(1)
                End If

            Else
                anEntryname = entryname.ToUpper
            End If

            '** return

            Return _ObjectEntryAttributes.ContainsKey(key:=anEntryname)
        End Function
        ''' <summary>
        ''' returns the schemaColumnAttribute for a given columnname and tablename
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectEntryAttribute(entryname As String, Optional onlyenabled As Boolean = True) As ormObjectEntryAttribute
            Dim anEntryname As String = ""
            Dim anObjectname As String = ""
            Dim names() As String = Shuffle.NameSplitter(entryname)

            '** split the names
            If names.Count > 1 Then
                anObjectname = names(0)
                If anObjectname <> _ObjectAttribute.ID Then
                    CoreMessageHandler(message:="object name of Object is not equal with entry name", arg1:=anObjectname, entryname:=entryname, _
                                       subname:="ObjectClassDescription.GetObjectEntryAttribute", messagetype:=otCoreMessageType.InternalWarning)
                    anEntryname = entryname.ToUpper
                    anObjectname = _ObjectAttribute.ID
                Else
                    anEntryname = names(1)
                End If

            Else
                anEntryname = entryname.ToUpper
            End If

                '** return

                If _ObjectEntryAttributes.ContainsKey(key:=anEntryname) Then
                    Dim anAttribute As ormObjectEntryAttribute = _ObjectEntryAttributes.Item(key:=anEntryname)
                    If onlyenabled AndAlso Not anAttribute.Enabled Then Return Nothing

                    '' substitute entries
                    _repository.SubstituteReferencedObjectEntry(attribute:=anAttribute)
                    '' set default values on non-set 
                    Me.SubstituteDefaultValues(attribute:=anAttribute)
                    ''return final
                    Return anAttribute
                Else
                    Return Nothing
                End If

        End Function
        ''' <summary>
        ''' returns a relation attribute by name (tablename is obsolete)
        ''' </summary>
        ''' <param name="relationname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetRelationAttribute(relationname As String, Optional onlyenabled As Boolean = False) As ormRelationAttribute
            Dim aRelationName As String = ""
            Dim names() As String = Shuffle.NameSplitter(relationname)

            '** split the names
            If names.Count > 1 Then
                aRelationName = names(1)
            Else
                aRelationName = relationname.ToUpper
                'If _TableAttributes.Count > 1 Then
                '    CoreMessageHandler(message:="more than one tables in the description but no table name specified in the relation name or as argument", _
                '                        messagetype:=otCoreMessageType.InternalWarning, subname:="ObjectClassDescription.GetRelationAttribute", _
                '                        arg1:=relationname)
                'End If
            End If

            '** return
            If _Relations.ContainsKey(key:=aRelationName) Then
                Dim anattribute As ormRelationAttribute = _Relations.Item(key:=aRelationName)
                If onlyenabled AndAlso Not anattribute.Enabled Then Return Nothing
                Return anattribute
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns a relation attribute by name (tablename is obsolete)
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetQueryAttribute(name As String, Optional onlyenabled As Boolean = True) As ormObjectQueryAttribute
            Dim aQueryname As String = ""
            Dim names() As String = Shuffle.NameSplitter(name)

            '** split the names
            If names.Count > 1 Then
                aQueryname = names(1)
            Else
                aQueryname = name.ToUpper
            End If

            '** return
            If _QueryAttributes.ContainsKey(key:=aQueryname) Then
                Dim anattribute As ormObjectQueryAttribute = _QueryAttributes.Item(key:=name.ToUpper)
                If onlyenabled AndAlso Not anattribute.Enabled Then Return Nothing
                Return anattribute
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' gets a List of all index attributes for a tablename
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetIndexAttributes(tablename As String, Optional onlyenabled As Boolean = True) As List(Of ormSchemaIndexAttribute)
            If Not onlyenabled Then Return _TableIndices.Item(key:=tablename).Values.ToList
            Return _TableIndices.Item(key:=tablename).Values.Where(Function(x) x.Enabled = True).ToList
        End Function
        ''' <summary>
        ''' gets the mapping attribute for a member name (of class)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntryMappingAttributes(membername As String, Optional onlyenabled As Boolean = True) As ormEntryMapping
            If _EntryMappings.ContainsKey(key:=membername) Then
                Dim anAttribute As ormEntryMapping = _EntryMappings.Item(key:=membername)
                If onlyenabled AndAlso Not anAttribute.Enabled Then Return Nothing
                Return anAttribute
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' gets the setter delegate for the member field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetFieldMemberSetterDelegate(membername As String) As Action(Of ormDataObject, Object)
            If _MappingSetterDelegates.ContainsKey(membername) Then
                Return _MappingSetterDelegates.Item(key:=membername)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' gets the getter delegate for the member field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetFieldMemberGetterDelegate(membername As String) As MappingGetterDelegate
            If _MappingGetterDelegates.ContainsKey(membername) Then
                Return _MappingGetterDelegates.Item(key:=membername)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' retrieves the Operation Caller Delegate for an operation
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetOperartionCallerDelegate(operationname As String) As OperationCallerDelegate
            If _OperationCallerDelegates.ContainsKey(operationname.ToUpper) Then
                Return _OperationCallerDelegates.Item(key:=operationname.ToUpper)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns the mapped FieldInfos for a given columnname and tablename
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMappedColumnFieldInfos(columnname As String, _
                                                  Optional tablename As String = "", _
                                                  Optional onlyenabled As Boolean = True) As List(Of FieldInfo)
            Dim aColumnname As String = ""
            Dim aTablename As String = ""
            If columnname Is Nothing Then
                CoreMessageHandler(message:="function called with nothing as columnname", subname:="ObjectClassDescription.GetMappedColumnFieldInfos", arg1:=Me.ObjectAttribute.ID, _
                                   messagetype:=otCoreMessageType.InternalError)
                Return New List(Of FieldInfo)
            End If
            Dim names() As String = Shuffle.NameSplitter(columnname)

            '** split the names
            If names.Count > 1 Then
                If tablename = "" Then
                    aTablename = names(0)
                Else
                    aTablename = tablename.ToUpper
                End If
                aColumnname = names(1)
            Else
                aColumnname = columnname.ToUpper
                aTablename = _TableAttributes.Keys.First
                If _TableAttributes.Count > 1 Then
                    CoreMessageHandler(message:="more than one tables in the description but no table name specified in the column name or as argument", _
                                       messagetype:=otCoreMessageType.InternalWarning, subname:="ObjectClassDescription.GetMappedColumnFieldInfos", _
                                       arg1:=columnname)
                End If
            End If

            '** return
            If _TableColumnsMappings.ContainsKey(key:=aTablename) Then
                ''' check on the enabled table
                If onlyenabled Then
                    If Not _TableAttributes.ContainsKey(aTablename) OrElse Not _TableAttributes.Item(key:=aTablename).Enabled Then
                        Return New List(Of FieldInfo)
                    End If

                End If
                If _TableColumnsMappings.Item(key:=aTablename).ContainsKey(key:=aColumnname) Then

                    Return _TableColumnsMappings.Item(key:=aTablename).Item(key:=aColumnname)
                Else
                    Return New List(Of FieldInfo)
                End If
            Else
                Return New List(Of FieldInfo)
            End If

        End Function
        ''' <summary>
        ''' returns the mapped FieldInfos for a given entryname
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntryFieldInfos(entryname As String, Optional onlyenabled As Boolean = True) As List(Of FieldInfo)
            Dim anObjectEntry = Me.GetObjectEntryAttribute(entryname:=entryname)
            If anObjectEntry Is Nothing OrElse (onlyenabled AndAlso Not anObjectEntry.Enabled) Then
                Return New List(Of FieldInfo)
            End If

            Dim aFieldname As String = anObjectEntry.ColumnName
            Dim aTablename As String = anObjectEntry.Tablename


            '** return
            If _TableColumnsMappings.ContainsKey(key:=aTablename) Then
                If _TableColumnsMappings.Item(key:=aTablename).ContainsKey(key:=aFieldname) Then
                    Return _TableColumnsMappings.Item(key:=aTablename).Item(key:=aFieldname)
                Else
                    Return New List(Of FieldInfo)
                End If
            Else
                Return New List(Of FieldInfo)
            End If

        End Function
        ''' <summary>
        ''' returns the FieldInfos for a given relation and tablename
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMappedRelationFieldInfos(relationName As String, _
                                                    Optional tablename As String = "", _
                                                    Optional onlyenabled As Boolean = True) As List(Of FieldInfo)
            Dim aRelationName As String = ""
            Dim aTablename As String = ""
            Dim names() As String = Shuffle.NameSplitter(relationName)

            '** split the names
            If names.Count > 1 Then
                If tablename = "" Then
                    aTablename = names(0)
                Else
                    aTablename = tablename.ToUpper
                End If
                aRelationName = names(1)
            Else
                aRelationName = relationName.ToUpper
                aTablename = _TableAttributes.Keys.First
                If _TableAttributes.Count > 1 Then
                    CoreMessageHandler(message:="more than one tables in the description but no table name specified in the column name or as argument", _
                                       messagetype:=otCoreMessageType.InternalWarning, subname:="ObjectClassDescription.GetMappedRelationFieldInfos", _
                                       arg1:=relationName)
                End If
            End If


            '** return
            If _TableRelationMappings.ContainsKey(key:=aTablename) Then
                If _TableRelationMappings.Item(key:=aTablename).ContainsKey(key:=aRelationName) Then
                    Return _TableRelationMappings.Item(key:=aTablename).Item(key:=aRelationName)
                Else
                    Return New List(Of FieldInfo)
                End If
            Else
                Return New List(Of FieldInfo)
            End If

        End Function

        ''' <summary>
        ''' gets a List of all column names for a given Table name
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetColumnNames(tablename As String, Optional onlyenabled As Boolean = True) As IList(Of String)
            If onlyenabled Then
                '' check the table and the object entries per table
                If Not _TableAttributes.ContainsKey(tablename.ToUpper) OrElse Not _TableAttributes.Item(tablename.ToUpper).Enabled _
                   OrElse Not _ObjectEntriesPerTable.ContainsKey(key:=tablename.ToUpper) Then
                    Return New List(Of String)
                End If

                Return _ObjectEntriesPerTable.Item(tablename.ToUpper).Values.Where(Function(x) x.Enabled = True)

            ElseIf _ObjectEntriesPerTable.ContainsKey(key:=tablename.ToUpper) Then

                Return _ObjectEntriesPerTable.Item(key:=tablename.ToUpper).Keys.ToList
            End If

            Return New List(Of String)
        End Function
        ''' <summary>
        ''' initialize a table attribute to the Description
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function InitializeTableAttribute(attribute As Attribute, tablename As String, overridesExisting As Boolean) As Boolean
            Dim aTableAttribute As ormSchemaTableAttribute = DirectCast(attribute, ormSchemaTableAttribute)
            Try

                '** Tables
                If _TableAttributes.ContainsKey(key:=tablename) And overridesExisting Then
                    _TableAttributes.Remove(key:=tablename)
                ElseIf _TableAttributes.ContainsKey(key:=tablename) And Not overridesExisting Then
                    Return True '* do nothing since we have a ClassOverrides tableattribute
                End If



                '** default values
                With aTableAttribute
                    .ID = tablename.ToUpper
                    '**
                    If Not .HasValueTableName Then .TableName = tablename.ToUpper
                    '** version
                    If Not .HasValueVersion Then .Version = 1
                    '** set the link
                    If _ObjectAttribute IsNot Nothing Then .ObjectID = _ObjectAttribute.ID
                End With
                '** check the table attribute from global store
                '** merge the values there
                '** table name must be set
                _repository.AlterTableAttribute(aTableAttribute)


                '** add it
                _TableAttributes.Add(key:=aTableAttribute.TableName, value:=aTableAttribute)
                '** to the object attributes
                If _ObjectAttribute.Tablenames Is Nothing OrElse _ObjectAttribute.Tablenames.Count = 0 Then
                    _ObjectAttribute.Tablenames = {aTableAttribute.TableName}
                Else
                    ReDim Preserve _ObjectAttribute.Tablenames(_ObjectAttribute.Tablenames.GetUpperBound(0) + 1)
                    _ObjectAttribute.Tablenames(_ObjectAttribute.Tablenames.GetUpperBound(0)) = aTableAttribute.TableName
                End If

                '** Add Columns per Table
                If _ObjectEntriesPerTable.ContainsKey(key:=aTableAttribute.TableName) Then _ObjectEntriesPerTable.Remove(key:=aTableAttribute.TableName)
                _ObjectEntriesPerTable.Add(key:=aTableAttribute.TableName, value:=New Dictionary(Of String, ormObjectEntryAttribute))
                '** Mappings per Table
                If _TableColumnsMappings.ContainsKey(key:=aTableAttribute.TableName) Then _TableColumnsMappings.Remove(key:=aTableAttribute.TableName)
                _TableColumnsMappings.Add(key:=aTableAttribute.TableName, value:=New Dictionary(Of String, List(Of FieldInfo)))
                '** Indices per Table
                If _TableIndices.ContainsKey(key:=aTableAttribute.TableName) Then _TableIndices.Remove(key:=aTableAttribute.TableName)
                _TableIndices.Add(key:=aTableAttribute.TableName, value:=New Dictionary(Of String, ormSchemaIndexAttribute))
                '** Relations per Table
                If _TableRelationMappings.ContainsKey(key:=aTableAttribute.TableName) Then _TableRelationMappings.Remove(key:=aTableAttribute.TableName)
                _TableRelationMappings.Add(key:=aTableAttribute.TableName, value:=New Dictionary(Of String, List(Of FieldInfo)))
                '** Relations per Table
                If _TableRelations.ContainsKey(key:=aTableAttribute.TableName) Then _TableRelations.Remove(key:=aTableAttribute.TableName)
                _TableRelations.Add(key:=aTableAttribute.TableName, value:=New Dictionary(Of String, ormRelationAttribute))

                Return True
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectClassDescription.InitializeTableAttribute")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' initialize a table attribute to the Description
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function InitializeQueryAttribute(attribute As Attribute, queryname As String, value As String, overridesExisting As Boolean) As Boolean
            Dim aQueryAttribute As ormObjectQueryAttribute = DirectCast(attribute, ormObjectQueryAttribute)
            Try
                If queryname = "" Then
                    queryname = value.ToUpper
                End If

                '** Tables
                If _QueryAttributes.ContainsKey(key:=queryname) And overridesExisting Then
                    _QueryAttributes.Remove(key:=queryname)
                ElseIf _QueryAttributes.ContainsKey(key:=queryname) And Not overridesExisting Then
                    Return True '* do nothing since we have a ClassOverrides attribute
                End If


                '** default values
                With aQueryAttribute
                    .ID = queryname.ToUpper
                    ' Entry names
                    If Not .HasValueEntrynames Then .AddAllFields = True
                    '** version
                    If Not .HasValueVersion Then .Version = 1
                End With

                '** add it
                _QueryAttributes.Add(key:=aQueryAttribute.ID, value:=aQueryAttribute)

                Return True
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectClassDescription.InitializeQueryAttribute")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' Initialize a ObjectEntry Attribute
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="name"></param>
        ''' <param name="tablename"></param>
        ''' <param name="fieldvalue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function InitializeObjectEntryAttribute(attribute As Attribute, name As String, tablename As String, fieldvalue As String, overridesExisting As Boolean) As Boolean
            Dim anObjectEntryName As String = ""
            Dim globaleTableAttributes As ormSchemaTableAttribute

            Try
                '* set the column name
                Dim anObjectEntryAttribute As ormObjectEntryAttribute = DirectCast(attribute, ormObjectEntryAttribute)

                If name = "" Then
                    name = fieldvalue
                    '** default
                    If anObjectEntryAttribute.Tablename Is Nothing OrElse anObjectEntryAttribute.Tablename = "" Then
                        If _TableAttributes.Count = 0 Then
                            CoreMessageHandler(message:="Object Entry Attribute was not assigned to a table - no tables seem to be defined in the class", _
                                               arg1:=_Type.Name, entryname:=fieldvalue, messagetype:=otCoreMessageType.InternalError, _
                                               subname:="ObjectClassDescription.InitializeObjectEntryAttribute", objectname:=_Type.Name)
                            Return False
                        End If
                        tablename = _TableAttributes.First.Key
                        If _TableAttributes.Count > 1 Then
                            CoreMessageHandler(message:="Object Entry Attribute was not assigned to a table although multiple tables are defined in class", _
                                               arg1:=_Type.Name, entryname:=fieldvalue, messagetype:=otCoreMessageType.InternalWarning, _
                                               subname:="ObjectClassDescription.InitializeObjectEntryAttribute", objectname:=_Type.Name)
                        End If
                    Else
                        tablename = anObjectEntryAttribute.Tablename
                    End If
                End If

                ' reset the attributes 
                If Not anObjectEntryAttribute.HasValueID Then anObjectEntryAttribute.ID = name.ToUpper
                If Not anObjectEntryAttribute.HasValueColumnName Then anObjectEntryAttribute.ColumnName = name.ToUpper
                If Not anObjectEntryAttribute.HasValueTableName Then anObjectEntryAttribute.Tablename = tablename.ToUpper
                If Not anObjectEntryAttribute.HasValueObjectName Then anObjectEntryAttribute.ObjectName = _ObjectAttribute.ID.ToUpper
                If Not anObjectEntryAttribute.HasValueEntryName Then anObjectEntryAttribute.EntryName = name.ToUpper
                If Not anObjectEntryAttribute.HasValueVersion Then anObjectEntryAttribute.Version = 1

                ' if we set an default value here - we cannot reference anymore :-(
                ' only possible for values which cannot be referenced !!
                ' put it in substitutedefaultvalues routine
                'If Not anObjectEntryAttribute.HasValueIsReadonly Then anObjectEntryAttribute.IsReadOnly = False
                'If Not anObjectEntryAttribute.HasValueIsNullable Then anObjectEntryAttribute.IsNullable = False
                'If Not anObjectEntryAttribute.HasValueIsUnique Then anObjectEntryAttribute.IsUnique = False

                If Not anObjectEntryAttribute.hasValuePosOrdinal Then
                    anObjectEntryAttribute.Posordinal = _ObjectEntryAttributes.Count + 1
                End If
                '**
                If Not name.Contains(".") AndAlso Not name.Contains(ConstDelimiter) Then
                    anObjectEntryName = _ObjectAttribute.ID.ToUpper & "." & name.ToUpper
                End If



                '* save to global
                If Not _ObjectEntryAttributes.ContainsKey(key:=name) Then
                    _ObjectEntryAttributes.Add(key:=name, value:=anObjectEntryAttribute)
                ElseIf Not overridesExisting Then
                ElseIf overridesExisting Then
                    _ObjectEntryAttributes.Remove(key:=name) ' if not enabled still please remove the entry
                    _ObjectEntryAttributes.Add(key:=name, value:=anObjectEntryAttribute)
                End If


                '** save in object description per Table as well as in global TableAttributes Store
                '** of the repository
                Dim aDictionary = _ObjectEntriesPerTable.Item(key:=tablename)
                If aDictionary IsNot Nothing Then
                    If Not aDictionary.ContainsKey(key:=anObjectEntryName) Then
                        aDictionary.Add(key:=anObjectEntryName, value:=anObjectEntryAttribute)
                        globaleTableAttributes = _repository.GetTableAttribute(tablename)
                        If globaleTableAttributes IsNot Nothing Then

                            If Not globaleTableAttributes.HasColumn(anObjectEntryAttribute.ColumnName) Then
                                globaleTableAttributes.AddColumn(anObjectEntryAttribute)
                            Else
                                globaleTableAttributes.UpdateColumn(anObjectEntryAttribute)
                            End If
                        Else
                            CoreMessageHandler(message:="table attribute was not defined in global table attribute store", arg1:=name, messagetype:=otCoreMessageType.InternalError, _
                                               subname:="ObjectClassDescription.InitializeObjectEntryAttribute", tablename:=tablename, objectname:=_Type.Name)

                        End If
                    ElseIf Not overridesExisting Then
                        '*** the existing should no be overridden
                        Return True
                    ElseIf overridesExisting Then
                        '*** override
                        aDictionary.Remove(key:=anObjectEntryName) '* through out
                        aDictionary.Add(key:=anObjectEntryName, value:=anObjectEntryAttribute) '* add new
                        globaleTableAttributes = _repository.GetTableAttribute(tablename)
                        If globaleTableAttributes IsNot Nothing Then
                            If globaleTableAttributes.GetColumn(anObjectEntryAttribute.ColumnName) Is Nothing Then
                                globaleTableAttributes.AddColumn(anObjectEntryAttribute)
                            End If
                        Else
                            CoreMessageHandler(message:="table attribute was not defined in global table attribute store", arg1:=name, messagetype:=otCoreMessageType.InternalError, _
                                               subname:="ObjectClassDescription.InitializeObjectEntryAttribute", tablename:=tablename, objectname:=_Type.Name)

                        End If
                    Else
                        CoreMessageHandler(message:="object entry exists in table more than once", arg1:=name, messagetype:=otCoreMessageType.InternalError, _
                                           subname:="ObjectClassDescription.InitializeObjectEntryAttribute", tablename:=tablename, objectname:=_Type.Name)
                    End If

                Else
                    CoreMessageHandler(message:="_tablecolumns does not exist", arg1:=tablename, messagetype:=otCoreMessageType.InternalError, _
                                       subname:="ObjectClassDescription.InitializeObjectEntryAttribute", objectname:=_Type.Name)
                End If


                ''' if not enabled delete the entry if we have one
                ''' doe it here so we could also do bookkeeping on deleting everything
                ''' BEWARE: Entries are stored under their FIELD VALUE = NAME not under the FIELD NAME (which are overwritten in the class)
                ''' 
                'If Not anObjectEntryAttribute.Enabled Then
                '    If overridesExisting Then
                '        If _ObjectEntryAttributes.ContainsKey(key:=name) Then _ObjectEntryAttributes.Remove(key:=name)
                '    End If
                '    Return True
                'End If

                '** create a foreign key attribute and store it with the global table
                '** use the reference object entry as foreign key reference
                If anObjectEntryAttribute.HasValueUseForeignKey AndAlso anObjectEntryAttribute.UseForeignKey <> otForeignKeyImplementation.None Then
                    If anObjectEntryAttribute.UseForeignKey <> otForeignKeyImplementation.None And _
                        Not anObjectEntryAttribute.HasValueForeignKeyReferences And anObjectEntryAttribute.HasValueReferenceObjectEntry Then
                        anObjectEntryAttribute.ForeignKeyReferences = {anObjectEntryAttribute.ReferenceObjectEntry}
                    ElseIf anObjectEntryAttribute.UseForeignKey <> otForeignKeyImplementation.None And _
                        Not anObjectEntryAttribute.HasValueForeignKeyReferences And Not anObjectEntryAttribute.HasValueReferenceObjectEntry Then
                        CoreMessageHandler(message:="For using foreign keys either the foreign key reference or the reference object entry is set", _
                                               arg1:=_Type.Name, entryname:=fieldvalue, messagetype:=otCoreMessageType.InternalWarning, _
                                               subname:="ObjectClassDescription.InitializeObjectEntryAttribute", objectname:=_Type.Name)
                    End If
                    '*** create and add
                    If globaleTableAttributes IsNot Nothing Then
                        Dim newForeignKey As New ormSchemaForeignKeyAttribute
                        With newForeignKey
                            .ID = "FK_" & globaleTableAttributes.TableName & "_" & anObjectEntryAttribute.ColumnName
                            If anObjectEntryAttribute.HasValueForeignKeyReferences Then .ForeignKeyReferences = anObjectEntryAttribute.ForeignKeyReferences
                            .Entrynames = {anObjectEntryAttribute.ObjectName & "." & anObjectEntryAttribute.ColumnName}
                            If anObjectEntryAttribute.HasValueForeignKeyProperties Then .ForeignKeyProperties = anObjectEntryAttribute.ForeignKeyProperties
                            .UseForeignKey = anObjectEntryAttribute.UseForeignKey
                            .Description = "created out of object entry " & anObjectEntryAttribute.ObjectName & "." & anObjectEntryAttribute.EntryName
                            .Version = anObjectEntryAttribute.Version
                            .ObjectID = anObjectEntryAttribute.ObjectName
                            .Tablename = anObjectEntryAttribute.Tablename
                        End With
                        '** add the foreign key
                        If Not globaleTableAttributes.HasForeignkey(newForeignKey.ID) Then
                            globaleTableAttributes.AddForeignKey(newForeignKey)
                        Else
                            CoreMessageHandler(message:="foreign key with ID '" & newForeignKey.ID & "' already exists in table attribute", _
                                              arg1:=newForeignKey.ID, tablename:=globaleTableAttributes.TableName, entryname:=fieldvalue, _
                                              messagetype:=otCoreMessageType.InternalWarning, _
                                              subname:="ObjectClassDescription.InitializeObjectEntryAttribute", objectname:=_Type.Name)
                        End If
                        Dim TablewiseDict As New Dictionary(Of String, ormSchemaForeignKeyAttribute)
                        If _ForeignKeys.ContainsKey(key:=tablename) Then
                            TablewiseDict = _ForeignKeys.Item(key:=tablename)
                        Else
                            _ForeignKeys.Add(key:=tablename, value:=TablewiseDict)
                        End If
                        If Not TablewiseDict.ContainsKey(key:=newForeignKey.ID) Then
                            TablewiseDict.Add(key:=newForeignKey.ID, value:=newForeignKey)
                        Else
                            CoreMessageHandler(message:="foreign key with ID '" & newForeignKey.ID & "' already exists in object class attribute", _
                                           arg1:=newForeignKey.ID, tablename:=globaleTableAttributes.TableName, entryname:=fieldvalue, _
                                           messagetype:=otCoreMessageType.InternalWarning, _
                                           subname:="ObjectClassDescription.InitializeObjectEntryAttribute", objectname:=_Type.Name)

                        End If
                    End If
                End If

                '** store the Primary Key also with the Object as Object Primary
                If anObjectEntryAttribute.HasValuePrimaryKeyOrdinal AndAlso _TableAttributes.Count = 1 Then
                    If Not _ObjectAttribute.HasValuePrimaryKeys Then
                        _ObjectAttribute.PrimaryKeys = {name.ToUpper}
                    Else
                        If _ObjectAttribute.PrimaryKeys.GetUpperBound(0) < anObjectEntryAttribute.PrimaryKeyOrdinal - 1 Then
                            ReDim Preserve _ObjectAttribute.PrimaryKeys(anObjectEntryAttribute.PrimaryKeyOrdinal - 1)
                        End If
                        _ObjectAttribute.PrimaryKeys(anObjectEntryAttribute.PrimaryKeyOrdinal - 1) = name.ToUpper
                    End If
                ElseIf anObjectEntryAttribute.HasValuePrimaryKeyOrdinal AndAlso _TableAttributes.Count > 1 Then
                    If _ObjectAttribute.PrimaryKeys Is Nothing OrElse _ObjectAttribute.PrimaryKeys.Count = 0 Then
                        CoreMessageHandler(message:="ATTENTION ! Primary keys for Object Attributes are not defined - multiple tables are used", _
                                           objectname:=_ObjectAttribute.ID, tablename:=tablename, messagetype:=otCoreMessageType.InternalError, _
                                           subname:="ObjectClassDescription.InitializeObjectEntryAttribute")
                    End If
                ElseIf anObjectEntryAttribute.HasValuePrimaryKeyOrdinal AndAlso _TableAttributes.Count = 0 Then
                    CoreMessageHandler(message:="ATTENTION ! Primary keys for Object Attributes are not defined - no tables are used", _
                                       objectname:=_ObjectAttribute.ID, tablename:=tablename, messagetype:=otCoreMessageType.InternalError, _
                                       subname:="ObjectClassDescription.InitializeObjectEntryAttribute")
                End If

                Return True
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectClassDescription.InitializeObjectEntryAttribute")
                Return False
            End Try



        End Function
        ''' <summary>
        ''' Initialize an ObjectEntry Mapping
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="tablename"></param>
        ''' <param name="value"></param>
        ''' <param name="fieldinfo"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function InitializeEntryMapping(attribute As Attribute, tablename As String, value As String, fieldinfo As FieldInfo, ClassOverrides As Boolean) As Boolean
            Try
                '* set the cloumn name
                Dim aMappingAttribute As ormEntryMapping = DirectCast(attribute, ormEntryMapping)
                '** default -> Table/Column mapping
                If Not aMappingAttribute.HasValueEntryName And Not aMappingAttribute.HasValueRelationName Then
                    CoreMessageHandler(message:="Entry Mapping Attribute was neither assigned to a data entry definition nor a relation definition", _
                                       arg1:=_Type.Name, entryname:=value, messagetype:=otCoreMessageType.InternalWarning, _
                                       subname:="ObjectClassDescription.InitializeEntryMapping", objectname:=_Type.Name)

                End If
               
                '** default -> Table/Column mapping
                If Not aMappingAttribute.HasValueTablename Then
                    tablename = _TableAttributes.First.Key
                    If _TableAttributes.Count > 1 Then
                        CoreMessageHandler(message:="Column Attribute was not assigned to a table although multiple tables are defined in class", _
                                           arg1:=_Type.Name, entryname:=value, messagetype:=otCoreMessageType.InternalWarning, objectname:=_Type.Name, _
                                           subname:="ObjectClassDescription.InitializeEntryMapping")
                    End If
                Else
                    tablename = aMappingAttribute.TableName
                End If
                ' reset the attributes 
                aMappingAttribute.TableName = tablename
                '** set the default columnname
                If aMappingAttribute.HasValueEntryName And Not aMappingAttribute.HasValueColumnName Then
                    If _ObjectEntryAttributes.ContainsKey(key:=aMappingAttribute.EntryName) Then
                        Dim anObjectEntry = _ObjectEntryAttributes.Item(key:=aMappingAttribute.EntryName)
                        aMappingAttribute.ColumnName = anObjectEntry.ColumnName
                    Else
                        CoreMessageHandler(message:="Object Entry  was not found", _
                                           arg1:=_Type.Name, entryname:=aMappingAttribute.EntryName, messagetype:=otCoreMessageType.InternalError, _
                                           subname:="ObjectClassDescription.InitializeEntryMapping", objectname:=_Type.Name)
                    End If

                End If
                '** save

                Dim aTablewiseDictionary As IDictionary
                Dim aGlobalDictionary As IDictionary
                Dim anID As String
                Dim aTablewiseID As String

                '***
                '*** ENTRY SETTING
                If aMappingAttribute.HasValueEntryName Then
                    aTablewiseDictionary = _TableColumnsMappings.Item(key:=tablename)
                    aGlobalDictionary = _ColumnEntryMapping
                    anID = aMappingAttribute.EntryName
                    aTablewiseID = aMappingAttribute.ColumnName

                    If aTablewiseDictionary Is Nothing Then
                        CoreMessageHandler(message:="_tablecolumnsMappings   does not exist", tablename:=tablename, arg1:=aMappingAttribute.ID, _
                                           messagetype:=otCoreMessageType.InternalError, _
                                           subname:="ObjectClassDescription.InitializeEntryMapping", objectname:=_Type.Name)
                        Return False
                    End If

                    If Not _ObjectEntryAttributes.ContainsKey(key:=anID) Then
                        CoreMessageHandler(message:="the to be mapped entry attribute does not exist", tablename:=tablename, _
                                           arg1:=aMappingAttribute.ID, _
                                          messagetype:=otCoreMessageType.InternalError, _
                                          subname:="ObjectClassDescription.InitializeEntryMapping", objectname:=_Type.Name)
                    Else
                        aMappingAttribute.Enabled = _ObjectEntryAttributes.Item(key:=anID).Enabled
                    End If
                    '***
                    '*** RELATION SETTING
                ElseIf aMappingAttribute.HasValueRelationName Then
                    aTablewiseDictionary = _TableRelationMappings.Item(key:=tablename)
                    aGlobalDictionary = _RelationEntryMapping
                    anID = aMappingAttribute.RelationName
                    aTablewiseID = anID

                    If aTablewiseDictionary Is Nothing Then
                        CoreMessageHandler(message:="_tablerelationMappings or  does not exist", tablename:=tablename, arg1:=aMappingAttribute.ID, _
                                           messagetype:=otCoreMessageType.InternalError, _
                                           subname:="ObjectClassDescription.InitializeEntryMapping", objectname:=_Type.Name)
                        Return False
                    End If

                    If Not _Relations.ContainsKey(key:=anID) Then
                        CoreMessageHandler(message:="the to be mapped entry attribute does not exist", tablename:=tablename, _
                                           arg1:=aMappingAttribute.ID, _
                                          messagetype:=otCoreMessageType.InternalError, _
                                          subname:="ObjectClassDescription.InitializeEntryMapping", objectname:=_Type.Name)
                    Else
                        aMappingAttribute.Enabled = _Relations.Item(key:=anID).Enabled
                    End If
                Else
                    CoreMessageHandler(message:="EntryMapping Attribute has no link to object entries nor relation", arg1:=aMappingAttribute.ID, _
                                       messagetype:=otCoreMessageType.InternalError, _
                                       subname:="ObjectClassDescription.InitializeEntryMapping", objectname:=_Type.Name)
                    Return False
                End If



                '** add the fieldinfo to the global list for per Mapping.ID (which is the entryname or the relationname)
                Dim aList As List(Of FieldInfo)
                If aGlobalDictionary.Contains(key:=anID) Then
                    aList = aGlobalDictionary.Item(key:=anID)
                Else
                    aList = New List(Of FieldInfo)
                    aGlobalDictionary.Add(key:=anID, value:=aList)
                End If
                If aList.Find(Function(x)
                                  Return x.Name = fieldinfo.Name
                              End Function) Is Nothing Then
                    aList.Add(fieldinfo)
                End If

                '** add the fieldinfo to the list for per Mapping.ID (which is the entryname or the relationname)
                aList = New List(Of FieldInfo)
                If aTablewiseDictionary.Contains(key:=aTablewiseID) Then
                    aList = aTablewiseDictionary.Item(key:=aTablewiseID)
                Else
                    aList = New List(Of FieldInfo)
                    aTablewiseDictionary.Add(key:=aTablewiseID, value:=aList)
                End If
                If aList.Find(Function(x)
                                  Return x.Name = fieldinfo.Name
                              End Function) Is Nothing Then
                    aList.Add(fieldinfo)
                End If

                '** defaults
                If aMappingAttribute.HasValueRelationName Then
                    If Not aMappingAttribute.HasValueInfuseMode Then aMappingAttribute.InfuseMode = otInfuseMode.OnInject Or otInfuseMode.OnDemand
                ElseIf aMappingAttribute.HasValueEntryName Or aMappingAttribute.HasValueColumnName Then
                    If Not aMappingAttribute.HasValueInfuseMode Then aMappingAttribute.InfuseMode = otInfuseMode.Always
                End If

                '** store the MappingAttribute under the fieldinfo name
                If Not _EntryMappings.ContainsKey(key:=fieldinfo.Name) Then
                    _EntryMappings.Add(key:=fieldinfo.Name, value:=aMappingAttribute)
                ElseIf ClassOverrides Then
                    Return True '* do nothing
                ElseIf Not ClassOverrides Then
                    _EntryMappings.Remove(key:=fieldinfo.Name)
                    _EntryMappings.Add(key:=fieldinfo.Name, value:=aMappingAttribute)
                Else
                    CoreMessageHandler(message:="Warning ! Field Member already associated with EntryMapping", arg1:=fieldinfo.Name, _
                                       objectname:=_Type.Name, messagetype:=otCoreMessageType.InternalWarning, subname:="ObjectClassDescription.InitializeEntryMapping")
                End If

                '*** create the setter
                If Not _MappingSetterDelegates.ContainsKey(key:=fieldinfo.Name) Then
                    Dim setter As Action(Of ormDataObject, Object) = CreateILGSetterDelegate(Of ormDataObject, Object)(_Type, fieldinfo)
                    _MappingSetterDelegates.Add(key:=fieldinfo.Name, value:=setter)
                End If
                '*** create the getter
                If Not _MappingGetterDelegates.ContainsKey(key:=fieldinfo.Name) Then
                    Dim getter = CreateILGGetterDelegate(Of Object, Object)(_Type, fieldinfo)
                    _MappingGetterDelegates.Add(key:=fieldinfo.Name, value:=getter)
                End If

                Return True
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectClassDescription.InitializeEntryMapping")
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Initialize a Relation Attribute to the Description
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="name"></param>
        ''' <param name="tablename"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function InitializeRelationAttribute(attribute As Attribute, name As String, tablename As String, value As String, overridesExisting As Boolean) As Boolean
            Try

                '* set the cloumn name
                Dim aRelationAttribute As ormRelationAttribute = DirectCast(attribute, ormRelationAttribute)
                If name = "" Then
                    name = value
                    '** default
                    If Not aRelationAttribute.HasValueTableName Then
                        tablename = _TableAttributes.First.Key
                        If _TableAttributes.Count > 1 Then
                            CoreMessageHandler(message:="Relation Attribute was not assigned to a table although multiple tables are defined in class", _
                                               arg1:=_Type.Name, entryname:=value, messagetype:=otCoreMessageType.InternalWarning, subname:="ObjectClassDescription.initializeRelationAttribute")
                        End If
                    Else
                        tablename = aRelationAttribute.TableName
                    End If
                End If
                ' reset the attributes 
                name = name.ToUpper

                aRelationAttribute.Name = name
                aRelationAttribute.TableName = tablename
                '* save to global
                If Not _Relations.ContainsKey(key:=name) Then
                    _Relations.Add(key:=name, value:=aRelationAttribute)
                ElseIf Not overridesExisting Then
                ElseIf overridesExisting Then
                    _Relations.Remove(key:=name)
                    _Relations.Add(key:=name, value:=aRelationAttribute)
                End If

                '** save to tablewise
                Dim aDictionary = _TableRelations.Item(key:=tablename)
                If aDictionary IsNot Nothing Then
                    If Not aDictionary.ContainsKey(key:=name) Then
                        aDictionary.Add(key:=name, value:=aRelationAttribute)
                    ElseIf Not overridesExisting Then
                        Return True '
                    ElseIf overridesExisting Then
                        aDictionary.Remove(key:=name)
                        aDictionary.Add(key:=name, value:=aRelationAttribute)
                    End If

                Else
                    CoreMessageHandler(message:="_tablerelations does not exist", arg1:=tablename, messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.initializeRelationAttribute")
                End If
                '** linkobject
                If Not aRelationAttribute.HasValueLinkedObject Then
                    CoreMessageHandler(message:="Relation Attribute has not defined a linked object type", objectname:=_Type.Name, _
                                       arg1:=name, messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.initializeRelationAttribute")
                Else
                    Dim atype As System.Type = aRelationAttribute.LinkObject
                    If atype.IsAbstract Then
                        CoreMessageHandler(message:="Relation Attribute with a linked object type which is abstract (mustinherit) is not supported", objectname:=_Type.Name, _
                                       arg1:=name, messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.initializeRelationAttribute")
                    End If

                End If

                If Not aRelationAttribute.HasValueLinkJOin AndAlso _
                Not (aRelationAttribute.HasValueFromEntries OrElse aRelationAttribute.HasValueToEntries) AndAlso _
                Not aRelationAttribute.HasValueToPrimarykeys Then
                    ' more possibilitues now e.g events or operation
                    'CoreMessageHandler(message:="Relation Attribute has not defined a link join or a matching entries or a target primary keys  - how to link ?", _
                    '                   objectname:=_Type.Name, _
                    '                   arg1:=name, messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.initializeRelationAttribute")
                End If
                If aRelationAttribute.HasValueFromEntries AndAlso aRelationAttribute.HasValueToEntries Then
                    If aRelationAttribute.ToEntries.Count > aRelationAttribute.FromEntries.Count Then
                        CoreMessageHandler(message:="relation attribute has nor mot ToEntries than FromEntries set", _
                                           arg1:=name, objectname:=_Type.Name, _
                                           subname:="ObjectClassDescription.initializeRelationAttribute", messagetype:=otCoreMessageType.InternalError)
                    End If
                End If

                '** defaults
                If Not aRelationAttribute.HasValueCascadeOnCreate Then aRelationAttribute.CascadeOnCreate = False
                If Not aRelationAttribute.HasValueCascadeOnDelete Then aRelationAttribute.CascadeOnDelete = False
                If Not aRelationAttribute.HasValueCascadeOnUpdate Then aRelationAttribute.CascadeOnUpdate = False


                Return True

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectClassDescription.InitializeRelationAttribute")
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Initialize a Relation Attribute to the Description
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="name"></param>
        ''' <param name="tablename"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function InitializeForeignKeyAttribute(attribute As Attribute, name As String, tablename As String, value As String, overridesExisting As Boolean) As Boolean
            Try

                '* set the cloumn name
                Dim aForeignKeyAttribute As ormSchemaForeignKeyAttribute = DirectCast(attribute, ormSchemaForeignKeyAttribute)
                If name = "" Then
                    name = value
                    '** default
                    If Not aForeignKeyAttribute.HasValueTableName Then
                        tablename = _TableAttributes.First.Key
                        If _TableAttributes.Count > 1 Then
                            CoreMessageHandler(message:="Relation Attribute was not assigned to a table although multiple tables are defined in class", _
                                               arg1:=_Type.Name, entryname:=value, messagetype:=otCoreMessageType.InternalWarning, _
                                               subname:="ObjectClassDescription.InitializeForeignKeyAttribute")
                        End If
                    Else
                        tablename = aForeignKeyAttribute.Tablename
                    End If
                End If

                ' reset the attributes 
                name = name.ToUpper
                aForeignKeyAttribute.ID = name
                aForeignKeyAttribute.Tablename = tablename
                If _ObjectAttribute.HasValueID Then aForeignKeyAttribute.ObjectID = _ObjectAttribute.ID

                '** save to table wise dictionary
                Dim aDictionary As New Dictionary(Of String, ormSchemaForeignKeyAttribute)
                If _ForeignKeys.ContainsKey(tablename) Then
                    aDictionary = _ForeignKeys.Item(key:=tablename)
                Else
                    _ForeignKeys.Add(key:=tablename, value:=aDictionary)
                End If

                If Not aDictionary.ContainsKey(key:=name) Then
                    aDictionary.Add(key:=name, value:=aForeignKeyAttribute)
                ElseIf Not overridesExisting Then
                    Return True '
                ElseIf overridesExisting Then
                    aDictionary.Remove(key:=name)
                    aDictionary.Add(key:=name, value:=aForeignKeyAttribute)
                End If

                '** save the table attribute
                If _TableAttributes.ContainsKey(tablename) Then
                    Dim aTableAttribute = _TableAttributes.Item(tablename)
                    If Not aTableAttribute.HasForeignkey(name) Then
                        aTableAttribute.AddForeignKey(aForeignKeyAttribute)
                    End If
                Else
                    CoreMessageHandler(message:="table attribute was not defined in global table attribute store", arg1:=name, _
                                       messagetype:=otCoreMessageType.InternalError, _
                                       subname:="ObjectClassDescription.InitializeForeignKeyAttribute", tablename:=tablename, objectname:=_Type.Name)

                End If

                '** save to global table attribute
                Dim globaleTableAttributes = _repository.GetTableAttribute(tablename)
                If globaleTableAttributes IsNot Nothing Then
                    If Not globaleTableAttributes.HasForeignkey(name) Then
                        globaleTableAttributes.AddForeignKey(aForeignKeyAttribute)
                    End If
                Else
                    CoreMessageHandler(message:="table attribute was not defined in global table attribute store", arg1:=name, _
                                       messagetype:=otCoreMessageType.InternalError, _
                                       subname:="ObjectClassDescription.InitializeForeignKeyAttribute", tablename:=tablename, objectname:=_Type.Name)

                End If


                '*** check the entrynames references
                '***
                If Not aForeignKeyAttribute.HasValueEntrynames Then
                    CoreMessageHandler(message:="entrynames must be defined in foreign key attribute", objectname:=_Type.Name, _
                                       arg1:=name, messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.InitializeForeignKeyAttribute")
                Else
                    For i = 0 To aForeignKeyAttribute.Entrynames.Count - 1
                        Dim areference As String = aForeignKeyAttribute.Entrynames(i)
                        Dim objectname As String
                        Dim entryname As String

                        If areference.Contains("."c) OrElse areference.Contains(ConstDelimiter) Then
                            Dim names = Shuffle.NameSplitter(areference)
                            objectname = names(0)
                            entryname = names(1)
                            If objectname.ToUpper <> aForeignKeyAttribute.ObjectID Then
                                CoreMessageHandler(message:="entrynames " & aForeignKeyAttribute.Entrynames.ToString & " in foreign key attribute must be defined for the object", objectname:=_Type.Name, _
                                      arg1:=objectname, messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.InitializeForeignKeyAttribute")
                            End If
                        Else
                            '** add the objectname
                            objectname = aForeignKeyAttribute.ObjectID
                            entryname = areference
                            aForeignKeyAttribute.Entrynames(i) = objectname.ToUpper & "." & entryname.ToUpper
                        End If

                        '** reference cannot be checked at this time
                        '**
                        'Dim anentry As ormObjectEntryAttribute = _repository.GetObjectEntryAttribute(entryname:=entryname, objectname:=objectname)
                        'If anentry Is Nothing Then
                        '    CoreMessageHandler(message:="entry reference object entry is not found the repository: '" & areference & "'", _
                        '             arg1:=name, objectname:=objectname, entryname:=entryname, _
                        '             messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.InitializeForeignKeyAttribute")
                        'End If
                    Next
                End If


                '*** check the foreign key references
                '***
                If Not aForeignKeyAttribute.HasValueForeignKeyReferences Then
                    CoreMessageHandler(message:="foreign key references must be defined in foreign key attribute", objectname:=_Type.Name, _
                                       arg1:=name, messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.InitializeForeignKeyAttribute")
                Else
                    For Each areference In aForeignKeyAttribute.ForeignKeyReferences
                        If Not areference.Contains("."c) AndAlso Not areference.Contains(ConstDelimiter) Then
                            CoreMessageHandler(message:="foreign key references must be [objectname].[entryname] in the foreign key attribute and not: '" & areference & "'", objectname:=_Type.Name, _
                                      arg1:=name, messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.InitializeForeignKeyAttribute")
                        Else
                            Dim names = Shuffle.NameSplitter(areference)
                            Dim objectname = names(0)
                            Dim entryname = names(1)
                            '** reference cannot be checked this time
                            '**
                            'Dim anentry As ormObjectEntryAttribute = _repository.GetObjectEntryAttribute(entryname:=entryname, objectname:=objectname)
                            'If anentry Is Nothing Then
                            '    CoreMessageHandler(message:="foreign key reference object entry is not found the repository: '" & areference & "'", objectname:=_Type.Name, _
                            '             arg1:=name, messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.InitializeForeignKeyAttribute")
                            'Else
                            'If Not anentry.HasValueTableName Then
                            '    CoreMessageHandler(message:="foreign key reference object entry has no tablename defined : '" & areference & "'", objectname:=_Type.Name, _
                            '         arg1:=name, messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.InitializeForeignKeyAttribute")
                            'Else
                            '    globaleTableAttributes = _repository.GetTableAttribute(anentry.Tablename)
                            '    If globaleTableAttributes IsNot Nothing Then
                            '        If Not globaleTableAttributes.HasColumn(anentry.ColumnName) Then
                            '            CoreMessageHandler(message:="In foreign key attribute the foreign key reference column was not defined in table", arg1:=name, _
                            '                               tablename:=anentry.Tablename, columnname:=anentry.ColumnName, _
                            '                               objectname:=objectname, entryname:=entryname, _
                            '                                messagetype:=otCoreMessageType.InternalError, _
                            '                                subname:="ObjectClassDescription.InitializeForeignKeyAttribute")
                            '        End If
                            '    Else
                            '        CoreMessageHandler(message:="In foreign key attribute the table was not defined in global table attribute store", arg1:=name, _
                            '                           messagetype:=otCoreMessageType.InternalError, _
                            '                             tablename:=anentry.Tablename, columnname:=anentry.ColumnName, _
                            '                              objectname:=objectname, entryname:=entryname, _
                            '                           subname:="ObjectClassDescription.InitializeForeignKeyAttribute")

                            '    End If
                            'End If
                            'End If
                        End If

                    Next
                End If

                '*** check number of entries
                If aForeignKeyAttribute.HasValueForeignKeyReferences AndAlso aForeignKeyAttribute.HasValueEntrynames Then
                    If aForeignKeyAttribute.ForeignKeyReferences.Count <> aForeignKeyAttribute.Entrynames.Count Then
                        CoreMessageHandler(message:="foreign key references must be the same number as entry names", objectname:=_Type.Name, _
                                           arg1:=name, messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.InitializeForeignKeyAttribute")
                    End If
                End If

                '** defaults
                If Not aForeignKeyAttribute.HasValueVersion Then aForeignKeyAttribute.Version = 1
                If Not aForeignKeyAttribute.HasValueUseForeignKey Then
                    aForeignKeyAttribute.UseForeignKey = otForeignKeyImplementation.None
                    CoreMessageHandler(message:="In foreign key attribute the use foreign key is not set - set to none", arg1:=name, _
                                                      messagetype:=otCoreMessageType.InternalWarning, _
                                                      tablename:=tablename, objectname:=_Type.Name, _
                                                      subname:="ObjectClassDescription.InitializeForeignKeyAttribute")
                End If

                If Not aForeignKeyAttribute.HasValueForeignKeyProperties Then
                    CoreMessageHandler(message:="In foreign key attribute the properties are not set - set to default", arg1:=name, _
                                                      messagetype:=otCoreMessageType.InternalWarning, _
                                                      tablename:=tablename, objectname:=_Type.Name, _
                                                      subname:="ObjectClassDescription.InitializeForeignKeyAttribute")
                    aForeignKeyAttribute.ForeignKeyProperties = {ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.Cascade & ")", _
                                                                 ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.Cascade & ")"}
                End If


                Return True

            Catch ex As Exception

                CoreMessageHandler(exception:=ex, subname:="ObjectClassDescription.InitializeForeignKeyAttribute")
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Initialize a Transaction Attribute to the Description
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="name"></param>
        ''' <param name="tablename"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function InitializeTransactionAttribute(attribute As Attribute, objectname As String, name As String, value As String, _
                                                      overridesExisting As Boolean) As Boolean
            Try

                '* set the  name
                Dim aTransactionAttribute As ormObjectTransactionAttribute = DirectCast(attribute, ormObjectTransactionAttribute)
                If name = "" Then
                    name = value
                End If
                If objectname = "" Then
                    objectname = _ObjectAttribute.ID
                End If
                ' reset the attributes 
                name = name.ToUpper
                '** default
                aTransactionAttribute.TransactionName = name
                If Not aTransactionAttribute.HasValueDefaultAllowPermission Then aTransactionAttribute.DefaultAllowPermission = True
                If Not aTransactionAttribute.HasValueID Then aTransactionAttribute.ID = name
                If Not aTransactionAttribute.HasValueVersion Then aTransactionAttribute.Version = 1
                '* save to global
                If Not _ObjectTransactionAttributes.ContainsKey(key:=name) Then
                    _ObjectTransactionAttributes.Add(key:=name, value:=aTransactionAttribute)
                ElseIf Not overridesExisting Then
                ElseIf overridesExisting Then
                    _ObjectTransactionAttributes.Remove(key:=name)
                    _ObjectTransactionAttributes.Add(key:=name, value:=aTransactionAttribute)
                End If

                '** validate rules
                If aTransactionAttribute.HasValuePermissionRules Then
                    For Each Rule In aTransactionAttribute.PermissionRules
                        Dim aProp As ObjectPermissionRuleProperty = New ObjectPermissionRuleProperty(Rule)
                        If Not aProp.Validate Then
                            CoreMessageHandler(message:="property rule did not validate", arg1:=name & "[" & Rule & "]", objectname:=_ObjectAttribute.ID, _
                                               subname:="ObjectClassDescription.InitializeTransactionAttribute", messagetype:=otCoreMessageType.InternalError)
                        End If
                    Next
                End If
                Return True

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectClassDescription.InitializeTransactionAttribute")
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Initialize the index Attribute
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="name"></param>
        ''' <param name="tablename"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function InitializeIndexAttribute(attribute As Attribute, name As String, tablename As String, value As String, overridesExisting As Boolean) As Boolean
            Try

                '* set the cloumn name
                Dim anIndexAttribute As ormSchemaIndexAttribute = DirectCast(attribute, ormSchemaIndexAttribute)
                If name = "" Then
                    name = value.ToUpper
                    '** default
                    If Not anIndexAttribute.HasValueTableName Then
                        tablename = _TableAttributes.First.Key
                        If _TableAttributes.Count > 1 Then
                            CoreMessageHandler(message:="Index Attribute was not assigned to a table although multiple tables are defined in class", _
                                               arg1:=_Type.Name, entryname:=value, messagetype:=otCoreMessageType.InternalWarning, subname:="ObjectClassDescription.Refresh")
                        End If
                    Else
                        tablename = anIndexAttribute.TableName
                    End If
                End If
                ' reset the attributes 

                anIndexAttribute.IndexName = name
                anIndexAttribute.TableName = tablename
                '* save to global
                If Not _Indices.ContainsKey(key:=name) Then
                    _Indices.Add(key:=name, value:=anIndexAttribute)
                ElseIf Not overridesExisting Then
                ElseIf overridesExisting Then
                    _Indices.Remove(key:=name)
                    _Indices.Add(key:=name, value:=anIndexAttribute)
                End If

                '** save
                Dim aDictionary = _TableIndices.Item(key:=tablename)
                If aDictionary IsNot Nothing Then
                    If Not aDictionary.ContainsKey(key:=name) Then
                        aDictionary.Add(key:=name, value:=anIndexAttribute)
                    ElseIf overridesExisting Then
                        Return True '** do nothing with the ClassOverrides one
                    ElseIf Not overridesExisting Then
                        aDictionary.Remove(key:=name)
                        aDictionary.Add(key:=name, value:=anIndexAttribute) '** overwrite the non-ClassOverrides
                    End If

                Else
                    CoreMessageHandler(message:="_tableindex does not exist", arg1:=tablename, messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.Refresh")
                End If


                Return True

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectClassDescription.InitializeRelationAttribute")
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Initialize the ObjectAttribute by a const field member of the class
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="fieldinfo"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function InitializeObjectAttributeByField(attribute As Attribute, fieldinfo As FieldInfo) As Boolean
            Try
                If _ObjectAttribute Is Nothing Then
                    _ObjectAttribute = attribute
                Else
                    With DirectCast(attribute, ormObjectAttribute)
                        If .HasValueDomainBehavior Then _ObjectAttribute.AddDomainBehavior = .AddDomainBehavior
                        If .HasValueClassname Then _ObjectAttribute.ClassName = .ClassName
                        If .HasValueDeleteFieldBehavior Then _ObjectAttribute.AddDeleteFieldBehavior = .AddDeleteFieldBehavior
                        If .HasValueDescription Then _ObjectAttribute.Description = .Description
                        If .HasValueDomainBehavior Then _ObjectAttribute.AddDomainBehavior = .AddDomainBehavior
                        If .HasValueID Then _ObjectAttribute.ID = .ID
                        If .HasValueIsActive Then _ObjectAttribute.IsActive = .IsActive
                        If .HasValueModulename Then _ObjectAttribute.Modulename = .Modulename
                        If .HasValueSpareFieldsBehavior Then _ObjectAttribute.AddSpareFieldsBehavior = .AddSpareFieldsBehavior
                        If .HasValuePrimaryKeys Then _ObjectAttribute.PrimaryKeys = .PrimaryKeys
                    End With
                End If

                '** defaults
                If Not _ObjectAttribute.HasValueClassname Then
                    _ObjectAttribute.ClassName = _Type.FullName
                End If
                If _ObjectAttribute.ID Is Nothing OrElse _ObjectAttribute.ID = "" Then
                    _ObjectAttribute.ID = fieldinfo.GetValue(Nothing).ToString.ToUpper
                End If
                If _ObjectAttribute.Modulename Is Nothing OrElse _ObjectAttribute.Modulename = "" Then
                    _ObjectAttribute.Modulename = _Type.Namespace.ToUpper
                End If
                If _ObjectAttribute.Description Is Nothing OrElse _ObjectAttribute.Description = "" Then
                    _ObjectAttribute.Description = ""
                End If

                Return True
            Catch ex As Exception
                CoreMessageHandler(subname:="ObjectClassDescription.InitializeFieldObjectEntryAttribute", exception:=ex)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Initialize a Transaction Attribute to the Description
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="name"></param>
        ''' <param name="tablename"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function InitializeOperationAttribute(attribute As Attribute, methodinfo As MethodInfo, _
                                                      overridesExisting As Boolean) As Boolean
            Try

                '* set the  name
                Dim anOperationAttribute As ormObjectOperationMethodAttribute = DirectCast(attribute, ormObjectOperationMethodAttribute)

                '** default
                anOperationAttribute.ID = methodinfo.Name.ToUpper
                anOperationAttribute.ClassDescription = Me

                If Not anOperationAttribute.HasValueOperationName Then anOperationAttribute.OperationName = methodinfo.Name.ToUpper
                anOperationAttribute.OperationName = anOperationAttribute.OperationName.ToUpper 'always to upper
                If Not anOperationAttribute.HasValueVersion Then anOperationAttribute.Version = 1
                anOperationAttribute.MethodInfo = methodinfo

                ''' check parameters
                If anOperationAttribute.HasValueParameterEntries Then
                    If anOperationAttribute.ParameterEntries.Count <> methodinfo.GetParameters.Count Then
                        CoreMessageHandler(message:="operation parameter count differs from method's parameter count", subname:="ObjectClassDescription.InitializeOperationAttribute", _
                                     messagetype:=otCoreMessageType.InternalWarning, arg1:=methodinfo.Name)
                    End If
                End If

                ''' check return parameters only if used in relation !
                ''' 
                If Me._Relations.Where(Function(x) (x.Value.HasValueCreateOperationID AndAlso x.Value.CreateOperation.ToUpper = Me.Name.ToUpper) OrElse (x.Value.HasValueRetrieveOperationID AndAlso x.Value.RetrieveOperation.ToUpper = Me.Name.ToUpper)).Count > 0 Then

                    Dim result As Boolean = False
                    Dim rtype As System.Type = methodinfo.ReturnType

                    If rtype.Equals(GetType(iormPersistable)) OrElse rtype.GetInterfaces.Contains(GetType(iormPersistable)) Then
                        result = True
                    ElseIf rtype.IsInterface AndAlso rtype.IsGenericType AndAlso _
                        (rtype.GetGenericTypeDefinition.Equals(GetType(IList(Of ))) OrElse rtype.GetGenericTypeDefinition.Equals(GetType(IEnumerable(Of ))) _
                         OrElse rtype.GetGenericTypeDefinition.Equals(GetType(iormRelationalCollection(Of )))
                            ) Then
                        result = True
                    ElseIf rtype.GetInterfaces.Contains(GetType(IList(Of ))) OrElse rtype.GetInterfaces.Contains(GetType(IEnumerable(Of ))) _
                        OrElse rtype.GetInterfaces.Contains(GetType(iormRelationalCollection(Of ))) Then
                        If rtype.GetGenericArguments(1).GetInterfaces.Equals(GetType(iormPersistable)) Then
                            result = True
                        Else
                            CoreMessageHandler(message:="generic return type is not of iormpersistable", subname:="ObjectClassDescription.InitializeOperationAttribute", _
                                          messagetype:=otCoreMessageType.InternalError, arg1:=methodinfo.Name)
                        End If
                    Else
                        CoreMessageHandler(message:="return type is not of iormpersistable or array, list, iormrelationalcollection nor dictionary", subname:="ObjectClassDescription.InitializeOperationAttribute", _
                                                     messagetype:=otCoreMessageType.InternalError, arg1:=methodinfo.Name)
                    End If
                End If

                '* generate the caller and save it
                Dim OperationDelegate = CreateILGMethodInvoker(methodinfo)

                If _OperationCallerDelegates.ContainsKey(anOperationAttribute.OperationName) Then
                    _OperationCallerDelegates.Remove(anOperationAttribute.OperationName)
                End If
                _OperationCallerDelegates.Add(key:=anOperationAttribute.OperationName, value:=OperationDelegate)

                '** save to description
                If Not _ObjectOperationAttributes.ContainsKey(key:=anOperationAttribute.OperationName) Then
                    _ObjectOperationAttributes.Add(key:=anOperationAttribute.OperationName, value:=anOperationAttribute)
                ElseIf Not overridesExisting Then
                ElseIf overridesExisting Then
                    _ObjectOperationAttributes.Remove(key:=anOperationAttribute.OperationName)
                    _ObjectOperationAttributes.Add(key:=anOperationAttribute.OperationName, value:=anOperationAttribute)
                End If

                '** store under Tag
                If anOperationAttribute.HasValueTag Then
                    If Not _ObjectOperationAttributesByTag.ContainsKey(key:=anOperationAttribute.Tag) Then
                        _ObjectOperationAttributesByTag.Add(key:=anOperationAttribute.Tag, value:=anOperationAttribute)
                    ElseIf Not overridesExisting Then
                    ElseIf overridesExisting Then
                        _ObjectOperationAttributesByTag.Remove(key:=anOperationAttribute.Tag)
                        _ObjectOperationAttributesByTag.Add(key:=anOperationAttribute.Tag, value:=anOperationAttribute)
                    End If
                End If

                Return True

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectClassDescription.InitializeOperationAttribute")
                Return False
            End Try
        End Function
        ''' <summary>
        ''' set the hook for the generic Retrieve
        ''' </summary>
        ''' <param name="methodinfo"></param>
        ''' <returns>True if the hook was set</returns>
        ''' <remarks></remarks>
        Private Function InitializeMethodRetrieveHook(methodinfo As MethodInfo) As Boolean
            '*
            If Not methodinfo.IsGenericMethodDefinition Then
                CoreMessageHandler(message:="retrieve is not a generic method in class", subname:="ObjectClassDescription.InitializeMethodRetrieveHook", _
                                   messagetype:=otCoreMessageType.InternalError, objectname:=methodinfo.GetBaseDefinition.Name)
                Return False
            End If

            Dim ahandle = methodinfo.MethodHandle
            Dim genericMethod = methodinfo.MakeGenericMethod({_Type})
            Dim parameters = genericMethod.GetParameters
            Dim retrieveParameters As ParameterInfo() = {}

            '     // compare the method parameters
            'if (parameters.Length == parameterTypes.Length) {
            '  for (int i = 0; i < parameters.Length; i++) {
            '    if (parameters[i].ParameterType != parameterTypes[i]) {
            '      continue; // this is not the method we're looking for
            '    }
            '  }

            If parameters.Count = 5 Then
                If _DataOperationHooks.ContainsKey(key:=ConstMTRetrieve) Then
                    _DataOperationHooks.Remove(key:=ConstMTRetrieve)
                End If
                _DataOperationHooks.Add(key:=ConstMTRetrieve, value:=genericMethod.MethodHandle)
                Return True
            End If
            Return False
        End Function
        ''' <summary>
        ''' Initialize the right CreateDataObject Function
        ''' </summary>
        ''' <param name="methodinfo"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function InitializeMethodCreateHook(methodinfo As MethodInfo) As Boolean
            '*
            If Not methodinfo.IsGenericMethodDefinition Then
                CoreMessageHandler(message:="CreateDataObject is not a generic method in class", subname:="ObjectClassDescription.InitializeMethodCreateHook", _
                                   messagetype:=otCoreMessageType.InternalError, objectname:=methodinfo.GetBaseDefinition.Name)
                Return False
            End If
            Dim genericMethod As MethodInfo = methodinfo.MakeGenericMethod({_Type})
            Dim parameters = genericMethod.GetParameters
            Dim retrieveParameters As ParameterInfo() = {}
            Dim found As Boolean = False

            '     // compare the method parameters
            'if (parameters.Length == parameterTypes.Length) {
            '  for (int i = 0; i < parameters.Length; i++) {
            '    if (parameters[i].ParameterType != parameterTypes[i]) {
            '      continue; // this is not the method we're looking for
            '    }
            '  }

            If parameters.Count = 4 Then

                For i = 0 To parameters.Length - 1
                    ' And parameters(i).ParameterType.IsArray doesnot work ?!
                    If parameters(i).ParameterType.Name.ToUpper = "Object[]&".ToUpper Then
                        found = True
                        Exit For
                    End If
                Next

                If Not found Then Return False

                '*** save
                If _DataOperationHooks.ContainsKey(key:=ConstMTCreateDataObject) Then
                    _DataOperationHooks.Remove(key:=ConstMTCreateDataObject)
                End If
                _DataOperationHooks.Add(key:=ConstMTCreateDataObject, value:=genericMethod.MethodHandle)
                Return True
            End If
            Return False
        End Function
        ''' <summary>
        ''' refresh all the loaded information
        ''' </summary>
        ''' <param name="force"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Initialize(Optional force As Boolean = False) As Boolean

            If Me._isInitalized AndAlso Not force Then Return False

            '** reset
            _ColumnEntryMapping.Clear()
            _TableAttributes.Clear()
            _Indices.Clear()
            _RelationEntryMapping.Clear()
            _TableColumnsMappings.Clear()
            _TableIndices.Clear()
            _ObjectEntriesPerTable.Clear()
            _TableRelationMappings.Clear()
            _Relations.Clear()
            _ObjectEntryAttributes.Clear()
            _ObjectAttribute = Nothing
            _DataOperationHooks.Clear()
            _EntryMappings.Clear()
            _ObjectTransactionAttributes.Clear()
            _ForeignKeys.Clear()
            _QueryAttributes.Clear()
            _ObjectOperationAttributes.Clear()
            _ObjectOperationAttributesByTag.Clear()
            '***
            '*** collect all the attributes first
            '***
            Dim aFieldList As System.Reflection.FieldInfo()
            Dim aName As String
            Dim aTablename As String
            Dim aValue As String

            Try

                SyncLock _lock

                    '** save the ObjectAttribute
                    For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(_Type)
                        If anAttribute.GetType().Equals(GetType(ormObjectAttribute)) Then
                            _ObjectAttribute = anAttribute
                            '** defaults
                            If Not _ObjectAttribute.HasValueClassname Then _ObjectAttribute.ClassName = _Type.FullName
                            If Not _ObjectAttribute.HasValueID Then _ObjectAttribute.ID = _Type.Name
                            If Not _ObjectAttribute.HasValueModulename Then _ObjectAttribute.Modulename = _Type.Namespace
                            If Not _ObjectAttribute.HasValueDescription Then _ObjectAttribute.Description = ""
                            If Not _ObjectAttribute.HasValueUseCache Then _ObjectAttribute.UseCache = False
                            If Not _ObjectAttribute.HasValueIsBootstap Then _ObjectAttribute.IsBootstrap = False
                            If Not _ObjectAttribute.HasValueIsActive Then _ObjectAttribute.IsActive = True
                            If Not _ObjectAttribute.HasValueTitle Then _ObjectAttribute.Title = _Type.Name
                            If Not _ObjectAttribute.HasValueVersion Then _ObjectAttribute.Version = 1
                        End If
                    Next

                    If _ObjectAttribute Is Nothing Then
                        CoreMessageHandler(message:="Class has no attribute - not added to repository", arg1:=_Type.Name, subname:="ObjectClassDescription.initialize", _
                                           messagetype:=otCoreMessageType.InternalError, objectname:=_Type.Name)
                        Return False
                    End If
                    '*** get the Attributes in the fields
                    '***
                    aFieldList = _Type.GetFields(Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Public Or _
                    Reflection.BindingFlags.Static Or Reflection.BindingFlags.Instance Or _
                    Reflection.BindingFlags.FlattenHierarchy)

                    '** look into each Const Type (Fields) to check for tablenames first !
                    '**


                    Dim overridesFlag As Boolean = False
                    For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList
                        If aFieldInfo.IsStatic AndAlso aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                            '** is this the declaring class ?! -> Do  override then
                            If aFieldInfo.DeclaringType = _Type Then
                                overridesFlag = True
                            Else
                                overridesFlag = False
                            End If
                            '** Attribtes
                            For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                                '** Object Attribute as Const bound
                                If anAttribute.GetType().Equals(GetType(ormObjectAttribute)) Then
                                    InitializeObjectAttributeByField(attribute:=anAttribute, fieldinfo:=aFieldInfo)

                                    '*** TABLE ATTRIBUTES
                                ElseIf anAttribute.GetType().Equals(GetType(ormSchemaTableAttribute)) Then
                                    ''' do we have the same const variable name herited from other classes ?
                                    ''' take then only the local / const variable with attributes from the herited class (overwriting)

                                    Dim localfield As FieldInfo = _Type.GetField(name:=aFieldInfo.Name, bindingAttr:=Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Public Or _
                                                  Reflection.BindingFlags.Static)
                                    If localfield Is Nothing OrElse (localfield IsNot Nothing AndAlso aFieldInfo.DeclaringType.Equals(localfield.ReflectedType)) Then

                                        If DirectCast(anAttribute, ormSchemaTableAttribute).TableName Is Nothing OrElse
                                        DirectCast(anAttribute, ormSchemaTableAttribute).TableName = "" Then
                                            aTablename = aFieldInfo.GetValue(Nothing).ToString.ToUpper
                                        End If
                                        If DirectCast(anAttribute, ormSchemaTableAttribute).Enabled Then InitializeTableAttribute(attribute:=anAttribute, tablename:=aTablename, overridesExisting:=overridesFlag)
                                    End If
                                End If

                            Next
                        End If
                    Next

                    '**
                    '** look up the definitions
                    '**
                    '*** get the Attributes in the fields
                    '***
                    aFieldList = _Type.GetFields(Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Public Or _
                                                 Reflection.BindingFlags.Static Or Reflection.BindingFlags.FlattenHierarchy)

                    For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList

                        If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                            '* see if this class is the declaring one
                            If aFieldInfo.DeclaringType = _Type Then
                                overridesFlag = True
                                '*** if this class is a derived one - override an existing one
                            Else 'If aFieldInfo.ReflectedType = _Type Then
                                overridesFlag = False
                            End If

                            '** Attributes
                            For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                                '** split the tablename if static value
                                aValue = ""
                                If aFieldInfo.IsStatic Then
                                    If aFieldInfo.GetValue(Nothing) IsNot Nothing Then
                                        aValue = aFieldInfo.GetValue(Nothing).ToString.ToUpper
                                    End If

                                    '* split
                                    '* beware a tableattribute would be lost
                                    Dim names As String() = Shuffle.NameSplitter(aValue)
                                    If names.Count > 1 Then
                                        aTablename = names(0)
                                        aName = names(1)
                                    Else
                                        aTablename = ""
                                        aName = ""
                                    End If
                                Else
                                    aTablename = ""
                                    aName = ""
                                End If

                                '** Object Entry Column
                                '**
                                If aFieldInfo.IsStatic AndAlso anAttribute.GetType().Equals(GetType(ormObjectEntryAttribute)) Then
                                    InitializeObjectEntryAttribute(attribute:=anAttribute, name:=aName, tablename:=aTablename, fieldvalue:=aValue, _
                                                              overridesExisting:=overridesFlag)
                                    '** Foreign Keys
                                ElseIf aFieldInfo.IsStatic AndAlso anAttribute.GetType().Equals(GetType(ormSchemaForeignKeyAttribute)) Then
                                    InitializeForeignKeyAttribute(attribute:=anAttribute, name:=aName, tablename:=aTablename, value:=aValue, overridesExisting:=overridesFlag)

                                    '** INDEX
                                ElseIf aFieldInfo.IsStatic AndAlso anAttribute.GetType().Equals(GetType(ormSchemaIndexAttribute)) Then
                                    InitializeIndexAttribute(attribute:=anAttribute, name:=aName, tablename:=aTablename, value:=aValue, overridesExisting:=overridesFlag)

                                    '** Relation
                                ElseIf aFieldInfo.IsStatic AndAlso anAttribute.GetType().Equals(GetType(ormRelationAttribute)) Then
                                    InitializeRelationAttribute(attribute:=anAttribute, name:=aName, tablename:=aTablename, value:=aValue, overridesExisting:=overridesFlag)

                                    '** Transaction
                                ElseIf aFieldInfo.IsStatic AndAlso anAttribute.GetType().Equals(GetType(ormObjectTransactionAttribute)) Then
                                    InitializeTransactionAttribute(attribute:=anAttribute, objectname:=aTablename, name:=aName, value:=aValue, overridesExisting:=overridesFlag)


                                    '** Queries
                                ElseIf aFieldInfo.IsStatic AndAlso anAttribute.GetType().Equals(GetType(ormObjectQueryAttribute)) Then

                                    InitializeQueryAttribute(attribute:=anAttribute, queryname:=aName, value:=aValue, overridesExisting:=overridesFlag)

                                End If

                            Next
                        End If
                    Next

                    '*** get the Attributes in the mapping fields
                    '***
                    aFieldList = _Type.GetFields(Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Public Or _
                                    Reflection.BindingFlags.Instance Or Reflection.BindingFlags.FlattenHierarchy)
                    '**
                    '** lookup the mappings from the definitions
                    '**
                    For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList

                        If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                            '* see if ClassOverrides from higher classes
                            If aFieldInfo.DeclaringType = _Type Then
                                overridesFlag = False
                            ElseIf aFieldInfo.ReflectedType = _Type Then
                                overridesFlag = True
                            End If

                            '** Attributes
                            For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                                '** split the tablename if static value
                                aValue = ""
                                If aFieldInfo.IsStatic Then
                                    If aFieldInfo.GetValue(Nothing) IsNot Nothing Then
                                        aValue = aFieldInfo.GetValue(Nothing).ToString.ToUpper
                                    End If

                                    '* split
                                    '* beware a tableattribute would be lost
                                    Dim names As String() = Shuffle.NameSplitter(aValue)
                                    If names.Count > 1 Then
                                        aTablename = names(0)
                                        aName = names(1)
                                    Else
                                        aTablename = ""
                                        aName = ""
                                    End If
                                Else
                                    aTablename = ""
                                    aName = ""
                                End If

                                '** ENTRY MAPPING -> instance
                                '**
                                If anAttribute.GetType().Equals(GetType(ormEntryMapping)) Then
                                    InitializeEntryMapping(attribute:=anAttribute, tablename:=aTablename, fieldinfo:=aFieldInfo, value:=aValue, ClassOverrides:=overridesFlag)
                                End If

                            Next
                        End If
                    Next

                    '** get some of the methods hooks
                    Dim theMethods = _Type.GetMethods(bindingAttr:=BindingFlags.FlattenHierarchy Or BindingFlags.Public Or BindingFlags.NonPublic Or _
                    BindingFlags.Static Or BindingFlags.Instance)
                    For Each aMethodInfo In theMethods

                        '* see if this class is the declaring one
                        If aMethodInfo.DeclaringType = _Type Then
                            overridesFlag = True
                            '*** if this class is a derived one - override an existing one
                        Else
                            overridesFlag = False
                        End If


                        ''' LEGACY SPECIAL HOOKS TO RETRIEVE / CREATEDATAOBJECT 
                        If aMethodInfo.Name.ToUpper = ConstMTRetrieve AndAlso aMethodInfo.IsGenericMethodDefinition Then
                            InitializeMethodRetrieveHook(methodinfo:=aMethodInfo)
                        ElseIf aMethodInfo.Name.ToUpper = ConstMTCreateDataObject AndAlso aMethodInfo.IsGenericMethodDefinition Then
                            InitializeMethodCreateHook(methodinfo:=aMethodInfo)
                        End If

                        '** Attributes
                        For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aMethodInfo)
                            If anAttribute.GetType().Equals(GetType(ormObjectOperationMethodAttribute)) Then
                                InitializeOperationAttribute(attribute:=anAttribute, methodinfo:=aMethodInfo, overridesExisting:=overridesFlag)
                            End If
                        Next
                    Next

                End SyncLock


                _isInitalized = True
                Return True
            Catch ex As Exception
                Call CoreMessageHandler(subname:="ObjectClassRepository.Initialize", exception:=ex)
                _isInitalized = False
                Return False
            End Try

        End Function

        ''' <summary>
        ''' generates an ILG Method Invoker from a method info
        ''' </summary>
        ''' <param name="methodInfo"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function CreateILGMethodInvoker(methodInfo As MethodInfo) As OperationCallerDelegate
            Dim dynamicMethod As New DynamicMethod(String.Empty, GetType(Object), New Type() {GetType(Object), GetType(Object())}, methodInfo.DeclaringType.[Module])
            Dim il As ILGenerator = dynamicMethod.GetILGenerator()
            Dim ps As ParameterInfo() = methodInfo.GetParameters()
            Dim paramTypes As Type() = New Type(ps.Length - 1) {}

            For i As Integer = 0 To paramTypes.Length - 1
                If ps(i).ParameterType.IsByRef Then
                    paramTypes(i) = ps(i).ParameterType.GetElementType()
                Else
                    paramTypes(i) = ps(i).ParameterType
                End If
            Next

            Dim locals As LocalBuilder() = New LocalBuilder(paramTypes.Length - 1) {}

            For i As Integer = 0 To paramTypes.Length - 1
                locals(i) = il.DeclareLocal(paramTypes(i), True)
            Next
            For i As Integer = 0 To paramTypes.Length - 1
                il.Emit(OpCodes.Ldarg_1)
                EmitFastInt(il, i)
                il.Emit(OpCodes.Ldelem_Ref)
                EmitCastToReference(il, paramTypes(i))
                il.Emit(OpCodes.Stloc, locals(i))
            Next
            If Not methodInfo.IsStatic Then
                il.Emit(OpCodes.Ldarg_0)
            End If
            For i As Integer = 0 To paramTypes.Length - 1
                If ps(i).ParameterType.IsByRef Then
                    il.Emit(OpCodes.Ldloca_S, locals(i))
                Else
                    il.Emit(OpCodes.Ldloc, locals(i))
                End If
            Next
            If methodInfo.IsStatic Then
                il.EmitCall(OpCodes.[Call], methodInfo, Nothing)
            Else
                il.EmitCall(OpCodes.Callvirt, methodInfo, Nothing)
            End If
            If methodInfo.ReturnType = GetType(System.Void) Then
                il.Emit(OpCodes.Ldnull)
            Else
                EmitBoxIfNeeded(il, methodInfo.ReturnType)
            End If

            For i As Integer = 0 To paramTypes.Length - 1
                If ps(i).ParameterType.IsByRef Then
                    il.Emit(OpCodes.Ldarg_1)
                    EmitFastInt(il, i)
                    il.Emit(OpCodes.Ldloc, locals(i))
                    If locals(i).LocalType.IsValueType Then
                        il.Emit(OpCodes.Box, locals(i).LocalType)
                    End If
                    il.Emit(OpCodes.Stelem_Ref)
                End If
            Next

            il.Emit(OpCodes.Ret)
            Dim invoder As OperationCallerDelegate = DirectCast(dynamicMethod.CreateDelegate(GetType(OperationCallerDelegate)), OperationCallerDelegate)
            Return invoder
        End Function


        Private Shared Sub EmitCastToReference(il As ILGenerator, type As System.Type)
            If type.IsValueType Then
                il.Emit(OpCodes.Unbox_Any, type)
            Else
                il.Emit(OpCodes.Castclass, type)
            End If
        End Sub
        Private Shared Sub EmitBoxIfNeeded(il As ILGenerator, type As System.Type)
            If type.IsValueType Then
                il.Emit(OpCodes.Box, type)
            End If
        End Sub

        Private Shared Sub EmitFastInt(il As ILGenerator, value As Integer)
            Select Case value
                Case -1
                    il.Emit(OpCodes.Ldc_I4_M1)
                    Return
                Case 0
                    il.Emit(OpCodes.Ldc_I4_0)
                    Return
                Case 1
                    il.Emit(OpCodes.Ldc_I4_1)
                    Return
                Case 2
                    il.Emit(OpCodes.Ldc_I4_2)
                    Return
                Case 3
                    il.Emit(OpCodes.Ldc_I4_3)
                    Return
                Case 4
                    il.Emit(OpCodes.Ldc_I4_4)
                    Return
                Case 5
                    il.Emit(OpCodes.Ldc_I4_5)
                    Return
                Case 6
                    il.Emit(OpCodes.Ldc_I4_6)
                    Return
                Case 7
                    il.Emit(OpCodes.Ldc_I4_7)
                    Return
                Case 8
                    il.Emit(OpCodes.Ldc_I4_8)
                    Return
            End Select

            If value > -129 AndAlso value < 128 Then
                il.Emit(OpCodes.Ldc_I4_S, Convert.ToSByte(value))
            Else
                il.Emit(OpCodes.Ldc_I4, value)
            End If
        End Sub


        Public Shared Function CreateILGCreateInstanceDelegate(constructor As ConstructorInfo, delegateType As Type) As CreateInstanceDelegate
            If constructor Is Nothing Then
                Throw New ArgumentNullException("constructor")
            End If
            If delegateType Is Nothing Then
                Throw New ArgumentNullException("delegateType")
            End If

            ' Validate the delegate return type
            Dim delMethod As MethodInfo = delegateType.GetMethod("Invoke")
            'If delMethod.ReturnType <> constructor.DeclaringType Then
            '       Throw New InvalidOperationException("The return type of the delegate must match the constructors declaring type")
            'End If

            ' Validate the signatures
            Dim delParams As ParameterInfo() = delMethod.GetParameters()
            Dim constructorParam As ParameterInfo() = constructor.GetParameters()
            If delParams.Length <> constructorParam.Length Then
                Throw New InvalidOperationException("The delegate signature does not match that of the constructor")
            End If
            For i As Integer = 0 To delParams.Length - 1
                ' Probably other things we should check ??
                If delParams(i).ParameterType <> constructorParam(i).ParameterType OrElse delParams(i).IsOut Then
                    Throw New InvalidOperationException("The delegate signature does not match that of the constructor")
                End If
            Next
            ' Create the dynamic method
            Dim method As New DynamicMethod(String.Format("{0}__{1}", constructor.DeclaringType.Name, Guid.NewGuid().ToString().Replace("-", "")), constructor.DeclaringType, Array.ConvertAll(Of ParameterInfo, Type)(constructorParam, Function(p) p.ParameterType), True)

            ' Create the il
            Dim gen As ILGenerator = method.GetILGenerator()
            For i As Integer = 0 To constructorParam.Length - 1
                If i < 4 Then
                    Select Case i
                        Case 0
                            gen.Emit(OpCodes.Ldarg_0)
                            Exit Select
                        Case 1
                            gen.Emit(OpCodes.Ldarg_1)
                            Exit Select
                        Case 2
                            gen.Emit(OpCodes.Ldarg_2)
                            Exit Select
                        Case 3
                            gen.Emit(OpCodes.Ldarg_3)
                            Exit Select
                    End Select
                Else
                    gen.Emit(OpCodes.Ldarg_S, i)
                End If
            Next
            gen.Emit(OpCodes.Newobj, constructor)
            gen.Emit(OpCodes.Ret)

            ' Return the delegate :)
            Return DirectCast(method.CreateDelegate(delegateType), CreateInstanceDelegate)

        End Function
      
        
        ''' <summary>
        ''' Searches an instanceType constructor with delegateType-matching signature and constructs delegate of delegateType creating new instance of instanceType.
        ''' Instance is casted to delegateTypes's return type. 
        ''' Delegate's return type must be assignable from instanceType.
        ''' </summary>
        ''' <param name="delegateType">Type of delegate, with constructor-corresponding signature to be constructed.</param>
        ''' <param name="instanceType">Type of instance to be constructed.</param>
        ''' <returns>Delegate of delegateType wich constructs instance of instanceType by calling corresponding instanceType constructor.</returns>
        Public Shared Function CreateLambdaInstance(delegateType As Type, instanceType As Type) As [Delegate]

            If Not GetType([Delegate]).IsAssignableFrom(delegateType) Then
                Throw New ArgumentException([String].Format("{0} is not a Delegate type.", delegateType.FullName), "delegateType")
            End If

            Dim invoke = delegateType.GetMethod("Invoke")
            Dim parameterTypes = invoke.GetParameters().[Select](Function(pi) pi.ParameterType).ToArray()
            Dim resultType = invoke.ReturnType
            If Not resultType.IsAssignableFrom(instanceType) Then
                Throw New ArgumentException([String].Format("Delegate's return type ({0}) is not assignable from {1}.", resultType.FullName, instanceType.FullName))
            End If

            Dim ctor = instanceType.GetConstructor(BindingFlags.Instance Or BindingFlags.[Public] Or BindingFlags.NonPublic, Nothing, parameterTypes, Nothing)
            If ctor Is Nothing Then
                Throw New ArgumentException("Can't find constructor with delegate's signature", "instanceType")
            End If

            Dim parapeters = parameterTypes.[Select](Function(x) Expression.Parameter(x)).ToArray()

            Dim newExpression = Expression.Lambda(delegateType, Expression.Convert(Expression.[New](ctor, parapeters), resultType), parapeters)
            Dim [delegate] = newExpression.Compile()
            Return [delegate]
        End Function
        ''' <summary>
        ''' create Instance
        ''' </summary>
        ''' <typeparam name="TDelegate"></typeparam>
        ''' <param name="instanceType"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Dim newList = Constructor.Compile(Of Func(Of Integer, IList(Of [String])))(GetType(List(Of [String])))
        ''' Dim list = newList(100)
        ''' </remarks>
        Public Shared Function CreateLambdaInstance(Of TDelegate)(instanceType As Type) As TDelegate
            Return DirectCast(DirectCast(CreateLambdaInstance(GetType(TDelegate), instanceType), Object), TDelegate)
        End Function
        ''' <summary>
        ''' Creates a IL GET VALUE
        ''' </summary>
        ''' <typeparam name="T">Type of the class of the setter variable</typeparam>
        ''' <typeparam name="TValue">Type of the value</typeparam>
        ''' <param name="field">fieldinfo </param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function CreateILGGetterDelegate(Of T, TValue)(tclass As Type, field As FieldInfo) As MappingGetterDelegate
            Try
                Dim m As New DynamicMethod("getter", GetType(TValue), New Type() {GetType(T)}, tclass)
                Dim cg As ILGenerator = m.GetILGenerator()

                ' Push the current value of the id field onto the 
                ' evaluation stack. It's an instance field, so load the
                ' instance  before accessing the field.
                cg.Emit(OpCodes.Ldarg_0)
                cg.Emit(OpCodes.Castclass, field.DeclaringType) 'cast the parameter of type object to the type containing the field

                cg.Emit(OpCodes.Ldfld, field)
                If field.FieldType.IsValueType Then
                    cg.Emit(OpCodes.Box, field.FieldType) 'box the value type, so you will have an object on the stack
                End If

                ' return
                cg.Emit(OpCodes.Ret)


                Return DirectCast(m.CreateDelegate(GetType(MappingGetterDelegate)), MappingGetterDelegate)
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectClassDescription.CreateILGetterDelegate")
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Creates a IL SET VALUE
        ''' </summary>
        ''' <typeparam name="T">Type of the class of the setter variable</typeparam>
        ''' <typeparam name="TValue">Type of the value</typeparam>
        ''' <param name="field">fieldinfo </param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function CreateILGSetterDelegate(Of T, TValue)(tclass As Type, field As FieldInfo) As Action(Of T, TValue)
            Try
                Dim m As New DynamicMethod("setter", GetType(System.Void), New Type() {GetType(T), GetType(TValue)}, tclass)
                Dim cg As ILGenerator = m.GetILGenerator()

                ' Load the instance , load the new value 
                ' of id, and store the new field value. 
                cg.Emit(OpCodes.Ldarg_0)
                cg.Emit(OpCodes.Castclass, field.DeclaringType) ' cast the parameter of type object to the type containing the field

                cg.Emit(OpCodes.Ldarg_1)
                If field.FieldType.IsValueType Then
                    cg.Emit(OpCodes.Unbox_Any, field.FieldType) ' unbox the value parameter to the value-type
                Else
                    cg.Emit(OpCodes.Castclass, field.FieldType) 'cast the value on the stack to the field type
                End If


                cg.Emit(OpCodes.Stfld, field)

                ' return
                cg.Emit(OpCodes.Ret)


                Return DirectCast(m.CreateDelegate(GetType(Action(Of T, TValue))), Action(Of T, TValue))
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectClassDescription.CreateILGSetterDelegate")
                Return Nothing
            End Try

        End Function

        'Private Shared Function CreateExpressionSetter(of T)(field As FieldInfo) As Object
        '    Dim targetExp As ParameterExpression = Expression.Parameter(GetType(T), "target")
        '    Dim valueExp As ParameterExpression = Expression.Parameter(GetType(String), "value")

        '    ' Expression.Property can be used here as well
        '    Dim fieldExp As MemberExpression = Expression.Field(targetExp, field)
        '    Dim assignExp As BinaryExpression = Expression.Assign(fieldExp, valueExp)

        '    Dim setter = Expression.Lambda(Of Action(Of T, String))(assignExp, targetExp, valueExp).Compile()

        '    setter(subject, "new value")
        'End Function


        'Private Shared Sub Main()
        '    Dim f As FieldInfo = GetType(MyObject).GetField("MyField")

        '    Dim setter As Action(Of MyObject, Integer) = CreateILGSetterDelegate(Of MyObject, Integer)(f)

        '    Dim obj = New MyObject()
        '    obj.MyField = 10

        '    setter(obj, 42)

        '    Console.WriteLine(obj.MyField)
        '    Console.ReadLine()
        'End Sub
    End Class

End Namespace