Option Explicit On

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

Namespace OnTrack


    ''' <summary>
    ''' store for attribute information in the dataobject classes - relies in the CORE
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ObjectClassRepository

        Private _isInitialized As Boolean = False
        Private _lock As New Object
        Private _BootStrapSchemaCheckSum As ULong

        '** stores
        Private _DescriptionsByClassTypeDescriptionStore As New Dictionary(Of String, ObjectClassDescription) 'name of classes with id
        Private _DescriptionsByIDDescriptionStore As New Dictionary(Of String, ObjectClassDescription) 'name of classes with id
        Private _Table2ObjectClassStore As New Dictionary(Of String, List(Of Type)) 'name of tables to types
        Private _BootstrapObjectClasses As New List(Of Type)
        Private _ClassDescriptorPerModule As New Dictionary(Of String, List(Of ObjectClassDescription))
        Private _TableAttributesStore As New Dictionary(Of String, ormSchemaTableAttribute)
        ''' <summary>
        ''' constructor of the object class repository
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()

        End Sub

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
        Public Function GetObjectClassType(objectname As String) As System.Type
            If _DescriptionsByClassTypeDescriptionStore.ContainsKey(key:=objectname.ToUpper) Then
                Return _DescriptionsByClassTypeDescriptionStore.Item(key:=objectname.ToUpper).Type
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
            Return GetObjectClassDescription([type].Name)
        End Function
        ''' <summary>
        ''' returns the ObjectClassDescription for a ObjectDescription Class by name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassDescription(typename As String) As ObjectClassDescription
            Me.Initialize()

            If _DescriptionsByClassTypeDescriptionStore.ContainsKey(key:=typename.ToUpper) Then
                Return _DescriptionsByClassTypeDescriptionStore.Item(key:=typename.ToUpper)
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
        Public Function GetReferenceTableColumn(ByRef attribute As ormSchemaTableColumnAttribute) As Boolean
            '*** REFERENCE OBJECT ENTRY
            If attribute.HasValueReferenceObjectEntry Then
                Dim refObjectName As String = ""
                Dim refObjectEntry As String = ""
                Dim names = attribute.ReferenceObjectEntry.ToUpper.Split({CChar(ConstDelimiter), "."c})
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
                        If .HasValueTypeID And Not attribute.HasValueTypeID Then attribute.Typeid = .Typeid
                        If .HasValueInnerTypeID And Not attribute.HasValueInnerTypeID Then attribute.InnerTypeid = .InnerTypeid
                        If .HasValueSize And Not attribute.HasValueSize Then attribute.Size = .Size
                        If .HasValueDescription And Not attribute.HasValueDescription Then attribute.Description = .Description
                        If .HasValueDefaultValue And Not attribute.HasValueDefaultValue Then attribute.DefaultValue = .DefaultValue
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
        Public Function GetReferenceObject(ByRef attribute As ormObjectEntryAttribute) As Boolean
            '*** REFERENCE OBJECT ENTRY
            If attribute.HasValueReferenceObjectEntry Then
                Dim refObjectName As String = ""
                Dim refObjectEntry As String = ""
                Dim names = attribute.ReferenceObjectEntry.ToUpper.Split({CChar(ConstDelimiter), "."c})
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
                        If GetReferenceTableColumn(attribute:=attribute) Then
                            If .HasValueEntryType And Not attribute.HasValueEntryType Then attribute.EntryType = .EntryType
                            If .HasValueTitle And Not attribute.HasValueTitle Then attribute.Title = .Title

                            If .HasValueAliases And Not attribute.HasValueAliases Then attribute.Aliases = .Aliases
                            If .HasValueProperties And Not attribute.HasValueProperties Then attribute.Properties = .Properties
                            If .HasValueVersion And Not attribute.HasValueVersion Then attribute.Version = .Version
                            If .HasValueSpareFieldTag And Not attribute.HasValueSpareFieldTag Then attribute.SpareFieldTag = .SpareFieldTag

                            If .HasValueRender And Not attribute.HasValueRender Then attribute.Render = .Render
                            If .HasValueRenderProperties And Not attribute.HasValueRenderProperties Then attribute.RenderProperties = .RenderProperties
                            If .HasValueRenderRegExpMatch And Not attribute.HasValueRenderRegExpMatch Then attribute.RenderRegExpMatch = .RenderRegExpMatch
                            If .HasValueRenderRegExpPattern And Not attribute.HasValueRenderRegExpPattern Then attribute.RenderRegExpPattern = .RenderRegExpPattern

                            If .HasValueValidate And Not attribute.HasValueValidate Then attribute.Validate = .Validate
                            If .HasValueLowerRange And Not attribute.HasValueLowerRange Then attribute.LowerRange = .LowerRange
                            If .HasValueUpperRange And Not attribute.HasValueUpperRange Then attribute.UpperRange = .UpperRange
                            If .HasValueValidationproperties And Not attribute.HasValueValidationproperties Then attribute.ValidationProperties = .ValidationProperties
                            If .HasValueLookupCondition And Not attribute.HasValueLookupCondition Then attribute.LookupCondition = .LookupCondition
                            If .HasValueValues And Not attribute.HasValueValues Then attribute.Values = .Values
                        End If

                    End With

                Else
                    CoreMessageHandler(message:="referenceObjectEntry  object id '" & refObjectName & "' and column name '" & refObjectEntry & "' not found for column schema", _
                                       entryname:=attribute.EntryName, objectname:=attribute.ObjectName, subname:="ObjectClassRepository.getReferenceObject", messagetype:=otCoreMessageType.InternalError)
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
            Dim names() As String = columnname.ToUpper.Split({CChar(ConstDelimiter), "."c})
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
                 GetReferenceTableColumn(attribute:=anAttribute) 
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
        Public Function GetObjectClassDescriptionsByTable(tablename As String) As List(Of ObjectClassDescription)
            Me.Initialize()
            Dim alist As New List(Of ObjectClassDescription)
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
        Public Function GetObjectClasses(tablename As String) As List(Of Type)
            Me.Initialize()
            If _Table2ObjectClassStore.ContainsKey(key:=tablename.ToUpper) Then
                Return _Table2ObjectClassStore.Item(key:=tablename.ToUpper)
            Else
                Return New List(Of Type)
            End If
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
                If _DescriptionsByClassTypeDescriptionStore.ContainsKey(aClass.Name.ToUpper) Then _DescriptionsByClassTypeDescriptionStore.Remove(key:=aClass.Name.ToUpper)
                Dim anewDescription As New ObjectClassDescription(aClass, Me)
                _DescriptionsByClassTypeDescriptionStore.Add(key:=anewDescription.Name.ToUpper, value:=anewDescription)

                '** object attributes
                For Each anAttribute As System.Attribute In aClass.GetCustomAttributes(False)
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
                        '** add to ObjectID
                        If _DescriptionsByIDDescriptionStore.ContainsKey(key:=anObjectAttribute.ID) Then
                            _DescriptionsByIDDescriptionStore.Remove(key:=anObjectAttribute.ID)
                        End If
                        _DescriptionsByIDDescriptionStore.Add(key:=anObjectAttribute.ID, value:=anewDescription)
                    End If
                Next

                ''' get the Fieldlist especially collect the constants
                aFieldList = aClass.GetFields(Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static)
                '** look into each Const Type (Fields)
                For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList
                    If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                        '** Attributes
                        For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                            If anAttribute.GetType().Equals(GetType(ormSchemaTableAttribute)) Then
                                Dim alist As List(Of Type)
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

                                '*** Object Attribute
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
        Public Delegate Function MappingGetter(dataobject As Object) As Object

        Private _Type As Type
        Private _ObjectAttribute As ormObjectAttribute
        Private _TableAttributes As New Dictionary(Of String, ormSchemaTableAttribute) 'name of table to Attribute
        Private _ObjectEntryAttributes As New Dictionary(Of String, ormObjectEntryAttribute) 'name of object entry to Attribute
        Private _ObjectOperationAttributes As New Dictionary(Of String, ormObjectOperationAttribute) 'name of object entry to Attribute
        Private _ObjectEntriesPerTable As New Dictionary(Of String, Dictionary(Of String, ormObjectEntryAttribute)) ' dictionary of tables to dictionary of columns
        Private _ColumnsPerTable As New Dictionary(Of String, Dictionary(Of String, ormSchemaTableColumnAttribute)) ' dictionary of tables to dictionary of columns

        Private _TableColumnsMappings As New Dictionary(Of String, Dictionary(Of String, List(Of FieldInfo))) ' dictionary of tables to dictionary of fieldmappings
        Private _ColumnEntryMapping As New Dictionary(Of String, List(Of FieldInfo)) ' dictionary of columns to mappings
        Private _MappingSetterDelegates As New Dictionary(Of String, Action(Of ormDataObject, Object)) ' dictionary of field to setter delegates
        Private _MappingGetterDelegates As New Dictionary(Of String, MappingGetter) ' dictionary of columns to mappings field to getter delegates

        Private _TableIndices As New Dictionary(Of String, Dictionary(Of String, ormSchemaIndexAttribute)) ' dictionary of tables to dictionary of indices
        Private _Indices As New Dictionary(Of String, ormSchemaIndexAttribute) ' dictionary of columns to mappings
        Private _TableRelationMappings As New Dictionary(Of String, Dictionary(Of String, List(Of FieldInfo))) ' dictionary of tables to dictionary of relation mappings
        Private _RelationEntryMapping As New Dictionary(Of String, List(Of FieldInfo)) ' dictionary of relations to mappings
        Private _TableRelations As New Dictionary(Of String, Dictionary(Of String, ormSchemaRelationAttribute)) ' dictionary of tables to dictionary of relation
        Private _Relations As New Dictionary(Of String, ormSchemaRelationAttribute) ' dictionary of relations to mappings
        Private _DataOperationHooks As New Dictionary(Of String, RuntimeMethodHandle)
        Private _EntryMappings As New Dictionary(Of String, ormEntryMapping)

        Private _ForeignKeys As New Dictionary(Of String, Dictionary(Of String, ormSchemaForeignKeyAttribute)) 'dictionary of tables and foreign keys by ids

        Private _isInitalized As Boolean = False
        Private _lock As New Object

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
        Public ReadOnly Property Keynames() As String()
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
                Return _TableAttributes.Keys.ToList
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
                Dim aList As New List(Of String)
                For Each aName In _ObjectEntryAttributes.Keys
                    If Not aList.Contains(aName) Then aList.Add(aName)
                Next
                Return aList
            End Get
        End Property
        ''' <summary>
        ''' gets a List of all column names
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ColumnNames As List(Of String)
            Get
                Dim aList As New List(Of String)
                For Each aTablename In _ObjectEntriesPerTable.Keys
                    Dim aList2 As List(Of String) = _ObjectEntriesPerTable.Item(key:=aTablename).Keys.ToList
                    For Each aColumnname In aList2
                        aList.Add(item:=aColumnname)
                    Next
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
        Public ReadOnly Property OperationAttributes As List(Of ormObjectOperationAttribute)
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
                For Each anAttribute In _ObjectEntryAttributes.Values
                    _repository.GetReferenceObject(attribute:=anAttribute)
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
                Dim aList As New List(Of String)
                For Each aTablename In _TableColumnsMappings.Keys
                    Dim aDir As Dictionary(Of String, List(Of FieldInfo)) = _TableColumnsMappings.Item(key:=aTablename)
                    For Each aColumnName In aDir.Keys
                        aList.Add(item:=aTablename & "." & aColumnName)
                    Next
                Next
                Return aList
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
                    Dim aList2 As List(Of ormSchemaIndexAttribute) = _TableIndices.Item(key:=aTablename).Values.ToList
                    aList.AddRange(aList2)
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
        Public ReadOnly Property RelationAttributes As List(Of ormSchemaRelationAttribute)
            Get
                Return _Relations.Values.ToList
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
                Return _TableAttributes.Values.ToList
            End Get
        End Property

        ''' <summary>
        ''' returns the SchemaTableAttribute for a table name
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSchemaTableAttribute(tablename As String) As ormSchemaTableAttribute
            If _TableAttributes.ContainsKey(key:=tablename) Then
                Return _TableAttributes.Item(tablename)
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
        ''' returns the schemaColumnAttribute for a given columnname and tablename
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectOperationAttribute(name As String) As ormObjectOperationAttribute
            Dim anEntryname As String = ""
            Dim anObjectname As String = ""
            Dim names() As String = name.ToUpper.Split({CChar(ConstDelimiter), "."c})

            '** split the names
            If names.Count > 1 Then
                anObjectname = names(0)
                If anObjectname <> _ObjectAttribute.ID Then
                    CoreMessageHandler(message:="object name of Object is not equal with entry name", arg1:=anObjectname, entryname:=name, _
                                       subname:="ObjectClassDescription.GetObjectOperationAttribute", messagetype:=otCoreMessageType.InternalWarning)
                End If
                anEntryname = names(1)
            Else
                anEntryname = name.ToUpper
            End If

            '** return

            If _ObjectOperationAttributes.ContainsKey(key:=anEntryname) Then
                Dim anAttribute As ormObjectOperationAttribute = _ObjectOperationAttributes.Item(key:=anEntryname)
                Return anAttribute
            Else
                Return Nothing
            End If

        End Function
        ''' <summary>
        ''' returns the schemaColumnAttribute for a given columnname and tablename
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectEntryAttribute(entryname As String) As ormObjectEntryAttribute
            Dim anEntryname As String = ""
            Dim anObjectname As String = ""
            Dim names() As String = entryname.ToUpper.Split({CChar(ConstDelimiter), "."c})

            '** split the names
            If names.Count > 1 Then
                anObjectname = names(0)
                If anObjectname <> _ObjectAttribute.ID Then
                    CoreMessageHandler(message:="object name of Object is not equal with entry name", arg1:=anObjectname, entryname:=entryname, _
                                       subname:="ObjectClassDescription.GetObjectEntryAttribute", messagetype:=otCoreMessageType.InternalWarning)
                End If
                anEntryname = names(1)
            Else
                anEntryname = entryname.ToUpper
            End If

            '** return

            If _ObjectEntryAttributes.ContainsKey(key:=anEntryname) Then
                Dim anAttribute As ormObjectEntryAttribute = _ObjectEntryAttributes.Item(key:=anEntryname)
                _repository.GetReferenceObject(attribute:=anAttribute)
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
        Public Function GetRelationAttribute(relationname As String) As ormSchemaRelationAttribute
            Dim aRelationName As String = ""
            Dim names() As String = relationname.ToUpper.Split({CChar(ConstDelimiter), "."c})

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
                Return _Relations.Item(key:=aRelationName)
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
        Public Function GetIndexAttributes(tablename As String) As List(Of ormSchemaIndexAttribute)
            Return _TableIndices.Item(key:=tablename).Values.ToList
        End Function
        ''' <summary>
        ''' gets a List of all index attributes for a tablename
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntryMappingAttributes(membername As String) As ormEntryMapping
            Return _EntryMappings.Item(key:=membername)
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
        Public Function GetFieldMemberGetterDelegate(membername As String) As MappingGetter
            If _MappingGetterDelegates.ContainsKey(membername) Then
                Return _MappingGetterDelegates.Item(key:=membername)
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
        Public Function GetMappedColumnFieldInfos(columnname As String, Optional tablename As String = "") As List(Of FieldInfo)
            Dim aFieldname As String = ""
            Dim aTablename As String = ""
            Dim names() As String = columnname.ToUpper.Split({CChar(ConstDelimiter), "."c})

            '** split the names
            If names.Count > 1 Then
                If tablename = "" Then
                    aTablename = names(0)
                Else
                    aTablename = tablename.ToUpper
                End If
                aFieldname = names(1)
            Else
                aFieldname = columnname.ToUpper
                aTablename = _TableAttributes.Keys.First
                If _TableAttributes.Count > 1 Then
                    CoreMessageHandler(message:="more than one tables in the description but no table name specified in the column name or as argument", _
                                       messagetype:=otCoreMessageType.InternalWarning, subname:="ObjectClassDescription.GetMappedColumnFieldInfos", _
                                       arg1:=columnname)
                End If
            End If

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
        ''' returns the mapped FieldInfos for a given columnname and tablename
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntryFieldInfos(entryname As String) As List(Of FieldInfo)
            Dim anObjectEntry = Me.GetObjectEntryAttribute(entryname:=entryname)
            If anObjectEntry Is Nothing Then
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
        Public Function GetMappedRelationFieldInfos(relationName As String, Optional tablename As String = "") As List(Of FieldInfo)
            Dim aRelationName As String = ""
            Dim aTablename As String = ""
            Dim names() As String = relationName.ToUpper.Split({CChar(ConstDelimiter), "."c})

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
        Public Function GetColumnNames(tablename As String) As List(Of String)
            Return _ObjectEntriesPerTable.Item(key:=tablename).Keys.ToList
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
                _TableRelations.Add(key:=aTableAttribute.TableName, value:=New Dictionary(Of String, ormSchemaRelationAttribute))

                Return True
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectClassDescription.InitializeTableAttribute")
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
               
                '**
                If Not name.Contains(".") AndAlso Not name.Contains(ConstDelimiter) Then
                    anObjectEntryName = _ObjectAttribute.ID.ToUpper & "." & name.ToUpper
                End If
                '* save to global
                If Not _ObjectEntryAttributes.ContainsKey(key:=name) Then
                    _ObjectEntryAttributes.Add(key:=name, value:=anObjectEntryAttribute)
                ElseIf Not overridesExisting Then
                ElseIf overridesExisting Then
                    _ObjectEntryAttributes.Remove(key:=name)
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
                            
                            If globaleTableAttributes.HasColumn(anObjectEntryAttribute.ColumnName) Then
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
                If Not _MappingGetterDelegates.Containskey(key:=fieldinfo.Name) Then
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
                Dim aRelationAttribute As ormSchemaRelationAttribute = DirectCast(attribute, ormSchemaRelationAttribute)
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
                End If
                If Not aRelationAttribute.HasValueLinkJOin AndAlso _
                Not (aRelationAttribute.HasValueFromEntries OrElse aRelationAttribute.HasValueToEntries) AndAlso _
                Not aRelationAttribute.HasValueToPrimarykeys Then
                    CoreMessageHandler(message:="Relation Attribute has not defined a link join or a matching entries or a target primary keys  - how to link ?", _
                                       objectname:=_Type.Name, _
                                       arg1:=name, messagetype:=otCoreMessageType.InternalError, subname:="ObjectClassDescription.initializeRelationAttribute")
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
                            Dim names = areference.ToUpper.Split("."c, ConstDelimiter)
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
                            Dim names = areference.ToUpper.Split("."c, ConstDelimiter)
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
        ''' Initialize a Operation Attribute to the Description
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="name"></param>
        ''' <param name="tablename"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function InitializeOperationAttribute(attribute As Attribute, objectname As String, name As String, value As String, _
                                                      overridesExisting As Boolean) As Boolean
            Try

                '* set the  name
                Dim aOperationAttribute As ormObjectOperationAttribute = DirectCast(attribute, ormObjectOperationAttribute)
                If name = "" Then
                    name = value
                End If
                If objectname = "" Then
                    objectname = _ObjectAttribute.ID
                End If
                ' reset the attributes 
                name = name.ToUpper
                '** default
                aOperationAttribute.OperationName = name
                If Not aOperationAttribute.HasValueDefaultAllowPermission Then aOperationAttribute.DefaultAllowPermission = True
                If Not aOperationAttribute.HasValueID Then aOperationAttribute.ID = name
                If Not aOperationAttribute.HasValueVersion Then aOperationAttribute.Version = 1
                '* save to global
                If Not _ObjectOperationAttributes.ContainsKey(key:=name) Then
                    _ObjectOperationAttributes.Add(key:=name, value:=aOperationAttribute)
                ElseIf Not overridesExisting Then
                ElseIf overridesExisting Then
                    _ObjectOperationAttributes.Remove(key:=name)
                    _ObjectOperationAttributes.Add(key:=name, value:=aOperationAttribute)
                End If

                '** validate rules
                If aOperationAttribute.HasValuePermissionRules Then
                    For Each Rule In aOperationAttribute.PermissionRules
                        Dim aProp As ObjectPermissionRuleProperty = New ObjectPermissionRuleProperty(Rule)
                        If Not aProp.Validate Then
                            CoreMessageHandler(message:="property rule did not validate", arg1:=name & "[" & Rule & "]", objectname:=_ObjectAttribute.ID, _
                                               subname:="ObjectClassDescription.InitializeOperationAttribute", messagetype:=otCoreMessageType.InternalError)
                        End If
                    Next
                End If
                Return True

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectClassDescription.InitializeOperationAttribute")
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
                        If .HasValueDomainBehavior Then _ObjectAttribute.AddDomainBehaviorFlag = .AddDomainBehaviorFlag
                        If .HasValueClassname Then _ObjectAttribute.ClassName = .ClassName
                        If .HasValueDeleteField Then _ObjectAttribute.DeleteFieldFlag = .DeleteFieldFlag
                        If .HasValueDescription Then _ObjectAttribute.Description = .Description
                        If .HasValueDomainBehavior Then _ObjectAttribute.AddDomainBehaviorFlag = .AddDomainBehaviorFlag
                        If .HasValueID Then _ObjectAttribute.ID = .ID
                        If .HasValueIsActive Then _ObjectAttribute.IsActive = .IsActive
                        If .HasValueModulename Then _ObjectAttribute.Modulename = .Modulename
                        If .HasValueSpareFields Then _ObjectAttribute.SpareFieldsFlag = .SpareFieldsFlag
                        If .HasValuePrimaryKeys Then _ObjectAttribute.PrimaryKeys = .PrimaryKeys

                    End With
                End If

                '** defaults
                If _ObjectAttribute.ClassName Is Nothing OrElse _ObjectAttribute.ClassName = "" Then
                    _ObjectAttribute.ClassName = _Type.Name.ToUpper
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
            _ObjectOperationAttributes.Clear()
            _ForeignKeys.Clear()
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
                            If Not _ObjectAttribute.HasValueClassname Then _ObjectAttribute.ClassName = _Type.Name
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
                                    If DirectCast(anAttribute, ormSchemaTableAttribute).TableName Is Nothing OrElse
                                    DirectCast(anAttribute, ormSchemaTableAttribute).TableName = "" Then
                                        aTablename = aFieldInfo.GetValue(Nothing).ToString.ToUpper
                                    End If
                                    InitializeTableAttribute(attribute:=anAttribute, tablename:=aTablename, overridesExisting:=overridesFlag)
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
                                    Dim names As String() = aValue.Split({CChar(ConstDelimiter), "."c})
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
                                ElseIf aFieldInfo.IsStatic AndAlso anAttribute.GetType().Equals(GetType(ormSchemaRelationAttribute)) Then
                                    InitializeRelationAttribute(attribute:=anAttribute, name:=aName, tablename:=aTablename, value:=aValue, overridesExisting:=overridesFlag)

                                    '** Operation
                                ElseIf aFieldInfo.IsStatic AndAlso anAttribute.GetType().Equals(GetType(ormObjectOperationAttribute)) Then
                                    InitializeOperationAttribute(attribute:=anAttribute, objectname:=aTablename, name:=aName, value:=aValue, overridesExisting:=overridesFlag)

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
                                    Dim names As String() = aValue.Split({CChar(ConstDelimiter), "."c})
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
                    For Each aMethod In theMethods
                        '*** RETRIEVE
                        If aMethod.Name.ToUpper = ConstMTRetrieve AndAlso aMethod.IsGenericMethodDefinition Then
                            InitializeMethodRetrieveHook(methodinfo:=aMethod)
                        ElseIf aMethod.Name.ToUpper = ConstMTCreateDataObject AndAlso aMethod.IsGenericMethodDefinition Then
                            InitializeMethodCreateHook(methodinfo:=aMethod)
                        End If
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
        ''' Creates a IL GET VALUE
        ''' </summary>
        ''' <typeparam name="T">Type of the class of the setter variable</typeparam>
        ''' <typeparam name="TValue">Type of the value</typeparam>
        ''' <param name="field">fieldinfo </param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function CreateILGGetterDelegate(Of T, TValue)(tclass As Type, field As FieldInfo) As MappingGetter
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


                Return DirectCast(m.CreateDelegate(GetType(MappingGetter)), MappingGetter)
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