﻿Option Explicit On

REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** COMMON BUSINESS OBJECT DEFINITION Classes 
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2013-08-08
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************''' <summary>

Imports System.Diagnostics.Debug
Imports System.Text.RegularExpressions
Imports OnTrack.Database
Imports OnTrack.Calendar

Namespace OnTrack.Commons


    ''' <summary>
    ''' Value Entry Class for List of Values
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=ValueEntry.ConstObjectID, modulename:=ConstModuleCore, Version:=1, Description:="lookup value pairs for general use", _
        useCache:=True, adddeletefieldbehavior:=True, addDomainBehavior:=True)> Public Class ValueEntry
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "ValueEntry"
        '** Table Schema
        <ormSchemaTableAttribute(Version:=1, usecache:=True)> Public Const ConstTableID As String = "tblDefValueEntries"

        '*** Primary Keys
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, primaryKeyordinal:=1, _
            properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty},
           XID:="VE2", title:="List", description:="ID of the list of values")> Const ConstFNListID = "ID"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, primaryKeyordinal:=2, _
            XID:="VE3", title:="Value", description:="value entry")> Const ConstFNValue = "VALUE"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=3 _
         , useforeignkey:=otForeignKeyImplementation.NativeDatabase, defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '*** Fields
        <ormObjectEntry(typeid:=otDataType.Text, size:=255, _
           XID:="VE4", title:="selector", description:="")> Const ConstFNSelector = "selector"

        <ormObjectEntry(typeid:=otDataType.Long, defaultvalue:=otDataType.Text, dbdefaultvalue:="3", _
          XID:="VE5", title:="datatype", description:="datatype of the  value")> Const ConstFNDatatype = "datatype"

        ' fields
        <ormEntryMapping(EntryName:=ConstFNDomainID)> Private _DomainID As String = ""
        <ormEntryMapping(EntryName:=ConstFNListID)> Private _ID As String = ""
        <ormEntryMapping(EntryName:=ConstFNSelector)> Private _selector As String = ""
        <ormEntryMapping(EntryName:=ConstFNValue)> Private _valuestring As String = ""
        <ormEntryMapping(EntryName:=ConstFNDatatype)> Private _datatype As otDataType = 0

        ''' <summary>
        ''' constructor of a clsOTDBDefWorkspace
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub

#Region "Properties"
        ''' <summary>
        ''' gets the ID of the Domain
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property DomainID() As String
            Get
                DomainID = _DomainID
            End Get

        End Property
        ''' <summary>
        ''' gets the ID of the Setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ListID() As String
            Get
                ListID = _ID
            End Get

        End Property
        ''' <summary>
        ''' Description of the setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Selector() As String
            Get
                Selector = _selector
            End Get
            Set(value As String)
                _selector = value
                Me.IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' returns the datatype 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Datatype As otDataType
            Set(value As otDataType)
                _datatype = value
            End Set
            Get
                Return _datatype
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the value of the domain setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Value As Object
            Set(value As Object)
                If value Is Nothing Then
                    _valuestring = ""
                Else
                    _valuestring = value.ToString
                End If
            End Set
            Get
                Try
                    Select Case _datatype
                        Case otDataType.Binary
                            Return CBool(_valuestring)
                        Case otDataType.Date, otDataType.Time, otDataType.Timestamp
                            If _valuestring Is Nothing Then Return constNullDate
                            If IsDate(_valuestring) Then Return CDate(_valuestring)
                            If _valuestring = constNullDate.ToString OrElse _valuestring = ConstNullTime.ToString Then Return constNullDate
                            If _valuestring = "" Then Return constNullDate
                        Case otDataType.List, otDataType.Memo, otDataType.Text
                            If _valuestring Is Nothing Then Return ""
                            Return CStr(_valuestring)
                        Case otDataType.Numeric
                            If IsNumeric(_valuestring) Then
                                Return CDbl(_valuestring)
                            Else
                                Return CDbl(0)
                            End If
                        Case otDataType.Long
                            If IsNumeric(_valuestring) Then
                                Return CDbl(_valuestring)
                            Else
                                Return CDbl(0)
                            End If
                            Return CLng(_valuestring)
                        Case Else
                            CoreMessageHandler(message:="data type not covered: " & _datatype, arg1:=_valuestring, subname:="ValueEntry.value", _
                                               messagetype:=otCoreMessageType.ApplicationError)
                            Return Nothing

                    End Select
                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, message:="could not convert value to data type " & _datatype, _
                                       arg1:=_valuestring, subname:="ValueEntry.value", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End Try

            End Get
        End Property
#End Region



        ''' <summary>
        ''' Retrieve the workspaceID Cache Object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal listID As String, ByVal value As Object, Optional ByVal domainID As String = "", Optional forcereload As Boolean = False) As ValueEntry
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim pkarray() As Object = {UCase(listID), value.ToString, UCase(domainID)}
            Dim anEntry As ValueEntry = Retrieve(Of ValueEntry)(pkArray:=pkarray, forceReload:=forcereload)
            '* try global domain
            If anEntry Is Nothing Then
                Dim pkglobalarray() As Object = {UCase(listID), value.ToString, UCase(ConstGlobalDomain)}
                Return Retrieve(Of ValueEntry)(pkArray:=pkglobalarray, forceReload:=forcereload)
            End If

        End Function
        ''' <summary>
        ''' Retrieve all value entries by list id in the domain
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function RetrieveByListID(ByVal listID As String, Optional ByVal domainID As String = "", Optional forcereload As Boolean = False) As List(Of ValueEntry)
            Dim aList As List(Of ValueEntry) = ormDataObject.AllDataObject(Of ValueEntry)(ID:="allbyListID", domainID:=domainID)
            Return aList
        End Function
        ''' <summary>
        ''' load and infuse the current value entry object
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(ByVal listid As String, ByVal value As Object, Optional ByVal domainID As String = "") As Boolean
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim primarykey() As Object = {UCase(listid), value.ToString, UCase(domainID)}
            If MyBase.Inject(primarykey) Then
                Return True
            Else
                Dim pkgloba() As Object = {UCase(listid), value.ToString, UCase(ConstGlobalDomain)}
                Return MyBase.Inject(pkgloba)
            End If
        End Function
        ''' <summary>
        ''' create the objects persistence schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of ValueEntry)(silent:=silent)
        End Function

        ''' <summary>
        ''' creates a new value entry for listid and value in the domain
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal listid As String, ByVal value As Object, Optional ByVal domainID As String = "") As Boolean
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim primarykey() As Object = {UCase(listid), value.ToString, UCase(domainID)}
            If MyBase.Create(primarykey, checkUnique:=True) Then
                _DomainID = UCase(domainID)
                _ID = UCase(listid)
                _valuestring = value.ToString
                Return True
            Else
                Return False
            End If
        End Function

    End Class

    ''' <summary>
    ''' Domain Setting Definition Class
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=DomainSetting.ConstObjectID, modulename:=ConstModuleCore, description:="properties per domain", _
        Version:=1, useCache:=True)> Public Class DomainSetting
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        '** const
        Public Const ConstObjectID = "DomainSetting"
        '** will be cached over domains
        <ormSchemaTableAttribute(adddeletefieldbehavior:=False, usecache:=False, Version:=1)> Public Const ConstTableID As String = "tblDefDomainSettings"

        <ormObjectEntry(XID:="DMS1", _
            referenceobjectentry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            title:="domain", Description:="domain identifier", defaultvalue:=ConstGlobalDomain, _
            primaryKeyordinal:=1, _
            useforeignkey:=otForeignKeyImplementation.ORM)> _
        Const ConstFNDomainID As String = Domain.ConstFNDomainID

        <ormObjectEntry(XID:="DMS2", _
           typeid:=otDataType.Text, size:=100, primaryKeyordinal:=2, _
           properties:={ObjectEntryProperty.Keyword}, _
           title:="Setting", description:="ID of the setting per domain")> _
        Const ConstFNSettingID = "id"

        <ormObjectEntry(XID:="DMS3", _
            typeid:=otDataType.Text, size:=100, title:="Description")> _
        Const ConstFNDescription = "desc"

        <ormObjectEntry(XID:="DMS4", typeid:=otDataType.Memo, _
           title:="value", description:="value of the domain setting in string presentation")> _
        Const ConstFNValue = "value"

        <ormObjectEntry(XID:="DMS5", typeid:=otDataType.Long, defaultvalue:=otDataType.Text, _
          title:="datatype", description:="datatype of the domain setting value")> _
        Const ConstFNDatatype = "datatype"

        ' fields
        <ormEntryMapping(EntryName:=ConstFNDomainID)> Private _DomainID As String = ""
        <ormEntryMapping(EntryName:=ConstFNSettingID)> Private _ID As String = ""
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNValue)> Private _valuestring As String = ""
        <ormEntryMapping(EntryName:=ConstFNDatatype)> Private _datatype As otDataType = 0
        ''' <summary>
        ''' constructor of a clsOTDBDefWorkspace
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub

#Region "Properties"
        ''' <summary>
        ''' gets the ID of the Domain
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property DomainID() As String
            Get
                DomainID = _DomainID
            End Get

        End Property
        ''' <summary>
        ''' gets the ID of the Setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ID() As String
            Get
                ID = _ID
            End Get

        End Property
        ''' <summary>
        ''' Description of the setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                _description = value
                Me.IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' returns the datatype 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Datatype As otDataType
            Set(value As otDataType)
                _datatype = value
            End Set
            Get
                Return _datatype
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the value of the domain setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property value As Object
            Set(value As Object)
                If value Is Nothing Then
                    _valuestring = ""
                Else
                    _valuestring = value.ToString
                End If
            End Set
            Get
                Try
                    Select Case _datatype
                        Case otDataType.Binary
                            Return CBool(_valuestring)
                        Case otDataType.Date, otDataType.Time, otDataType.Timestamp
                            If _valuestring Is Nothing Then Return constNullDate
                            If IsDate(_valuestring) Then Return CDate(_valuestring)
                            If _valuestring = constNullDate.ToString OrElse _valuestring = ConstNullTime.ToString Then Return constNullDate
                            If _valuestring = "" Then Return constNullDate
                        Case otDataType.List, otDataType.Memo, otDataType.Text
                            If _valuestring Is Nothing Then Return ""
                            Return CStr(_valuestring)
                        Case otDataType.Numeric
                            If IsNumeric(_valuestring) Then
                                Return CDbl(_valuestring)
                            Else
                                Return CDbl(0)
                            End If
                        Case otDataType.Long
                            If IsNumeric(_valuestring) Then
                                Return CDbl(_valuestring)
                            Else
                                Return CDbl(0)
                            End If
                            Return CLng(_valuestring)
                        Case Else
                            CoreMessageHandler(message:="data type not covered: " & _datatype, arg1:=_valuestring, subname:="DomainSetting.value", _
                                               messagetype:=otCoreMessageType.ApplicationError)
                            Return Nothing

                    End Select
                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, message:="could not convert value to data type " & _datatype, _
                                       arg1:=_valuestring, subname:="DomainSetting.value", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End Try

            End Get
        End Property
#End Region



        ''' <summary>
        ''' Retrieve the workspaceID Cache Object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal domainID As String, ByVal id As String, Optional forcereload As Boolean = False) As DomainSetting
            Dim pkarray() As Object = {UCase(domainID), UCase(id)}
            Return Retrieve(Of DomainSetting)(pkArray:=pkarray, forceReload:=forcereload)
        End Function
        ''' <summary>
        ''' Retrieve the workspaceID Cache Object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function RetrieveByDomain(ByVal domainID As String, Optional forcereload As Boolean = False) As List(Of DomainSetting)
            Dim aList As List(Of DomainSetting) = ormDataObject.AllDataObject(Of DomainSetting)(ID:="allbyDomain", _
                                                                                      where:="[" & ConstFNDomainID & "] = @" & ConstFNDomainID, _
                                                                                      parameters:={New ormSqlCommandParameter(id:="@" & ConstFNDomainID, columnname:=ConstFNDomainID, tablename:=ConstTableID, value:=domainID)}.ToList)
            Return aList
        End Function
        ''' <summary>
        ''' load and infuse the current workspaceID object
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(ByVal domainID As String, ByVal id As String) As Boolean
            Dim primarykey() As Object = {UCase(Trim(domainID)), UCase(id)}
            Return MyBase.Inject(primarykey)
        End Function
        ''' <summary>
        ''' create the objects persistence schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of DomainSetting)(silent:=silent)
        End Function

        ''' <summary>
        ''' creates with this object a new persistable Def workspaceID
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal domainID As String, ByVal id As String) As DomainSetting
            Dim primarykey() As Object = {UCase(domainID), UCase(id)}
            Return ormDataObject.CreateDataObject(Of DomainSetting)(primarykey, checkUnique:=False)
        End Function

    End Class
    ''' <summary>
    ''' User Definition Class of an OnTrack User
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=Group.ConstObjectID, description:="group definition for users", _
        modulename:=ConstModuleCore, Version:=1, usecache:=True, adddomainbehavior:=True, adddeletefieldbehavior:=True, isbootstrap:=False)> _
    Public Class Group
        Inherits ormDataObject


        '*** Object ID
        Public Const ConstObjectID = "Group"

        '*** Schema Table
        <ormSchemaTable(version:=1, usecache:=True)> Public Const ConstTableID As String = "tblDefGroups"

        '*** Primary Keys
        <ormObjectEntry(typeid:=otDataType.Text, size:=50, primarykeyordinal:=1, _
            properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty},
          XID:="G1", title:="Group", description:="name of the OnTrack user group")> Public Const ConstFNGroupname = "groupname"
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=2, _
                       defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '*** Fields
        <ormObjectEntry(typeid:=otDataType.Text, size:=255, _
        XID:="G5", title:="description", description:="description of the OnTrack user group")> Public Const ConstFNDescription = "desc"

        <ormObjectEntry(referenceobjectentry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, isnullable:=True, _
            XID:="G10", title:="Default Workspace", description:="default workspace of the OnTrack user")> Public Const ConstFNDefaultWorkspace = "defws"
        <ormObjectEntry(referenceobjectentry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, isnullable:=True, _
           XID:="G11", title:="Default Domain", description:="default domain of the OnTrack user")> Public Const ConstFNDefaultDomainID = "defdomain"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, _
           XID:="UR1", title:="Alter Schema Right", description:="has user the right to alter the database schema")> _
        Public Const ConstFNAlterSchema = "alterschema"
        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, _
          XID:="UR2", title:="Update Data Right", description:="has user the right to update data (new/change/delete)")> _
        Public Const ConstFNUpdateData = "updatedata"
        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=True, _
          XID:="UR3", title:="Read Data Right", description:="has user the right to read the database data")> Public Const ConstFNReadData = "readdata"
        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, _
          XID:="UR4", title:="No Access", description:="has user no access")> Public Const ConstFNNoAccess = "noright"

        '* Relations
        '* Members
        <ormSchemaRelation(cascadeOnDelete:=True, cascadeonUpdate:=True, FromEntries:={ConstFNGroupname}, toEntries:={GroupMember.ConstFNGroupname}, _
            LinkObject:=GetType(GroupMember))> Const ConstRelMembers = "members"
        <ormEntryMapping(Relationname:=ConstRelMembers, infusemode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand)> _
        Private _groupmembers As New List(Of GroupMember)

        'fields
        <ormEntryMapping(EntryName:=ConstFNGroupname)> Private _groupname As String
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _desc As String

        <ormEntryMapping(EntryName:=ConstFNDefaultWorkspace)> Private _DefaultWorkspaceID As String
        <ormEntryMapping(EntryName:=ConstFNDefaultDomainID)> Private _DefaultDomainID As String

        <ormEntryMapping(EntryName:=ConstFNReadData)> Private _hasRead As Boolean
        <ormEntryMapping(EntryName:=ConstFNUpdateData)> Private _hasUpdate As Boolean
        <ormEntryMapping(EntryName:=ConstFNAlterSchema)> Private _hasAlterSchema As Boolean
        <ormEntryMapping(EntryName:=ConstFNNoAccess)> Private _hasNoRights As Boolean


#Region "Properties"



        Public Property Description() As String
            Get
                Description = _desc
            End Get
            Set(ByVal avalue As String)
                SetValue(entryname:=ConstFNDescription, value:=avalue)
            End Set
        End Property

        Public Property DefaultWorkspaceID As String

            Get
                DefaultWorkspaceID = _DefaultWorkspaceID
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNDefaultWorkspace, value:=value)
            End Set
        End Property

        ReadOnly Property GroupName() As String
            Get
                GroupName = _groupname
            End Get
        End Property
        ''' <summary>
        ''' has no rights at all ?! -> Blocked ?!
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property HasNoRights() As Boolean
            Get
                HasNoRights = _hasNoRights
            End Get
            Set(value As Boolean)
                _hasNoRights = value
                IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' has right to read
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property HasReadRights() As Boolean
            Get
                HasReadRights = _hasRead
            End Get
            Set(value As Boolean)
                _hasRead = value
                IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' has right to update and read data
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property HasUpdateRights() As Boolean
            Get
                HasUpdateRights = _hasUpdate
            End Get
            Set(value As Boolean)
                _hasUpdate = value
                IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' Has Right to update, read and alter schema data
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property HasAlterSchemaRights() As Boolean
            Get
                HasAlterSchemaRights = _hasAlterSchema
            End Get
            Set(value As Boolean)
                _hasAlterSchema = value
                IsChanged = True
            End Set
        End Property

        ''' <summary>
        ''' gets the accessright out of the setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property AccessRight As otAccessRight
            Get
                '* highes right first
                If Me.HasAlterSchemaRights Then
                    Return otAccessRight.AlterSchema
                ElseIf Me.HasUpdateRights Then
                    Return otAccessRight.ReadUpdateData
                ElseIf Me.HasReadRights Then
                    Return otAccessRight.ReadOnly
                End If

                Return otAccessRight.Prohibited
            End Get
            Set(value As otAccessRight)
                Select Case value
                    Case otAccessRight.AlterSchema
                        Me.HasAlterSchemaRights = True
                        Me.HasUpdateRights = True
                        Me.HasReadRights = True
                        Me.HasNoRights = False
                    Case otAccessRight.ReadUpdateData
                        Me.HasAlterSchemaRights = False
                        Me.HasUpdateRights = True
                        Me.HasReadRights = True
                        Me.HasNoRights = False
                    Case otAccessRight.ReadOnly
                        Me.HasAlterSchemaRights = False
                        Me.HasUpdateRights = False
                        Me.HasReadRights = True
                        Me.HasNoRights = False
                    Case otAccessRight.Prohibited
                        Me.HasAlterSchemaRights = False
                        Me.HasUpdateRights = False
                        Me.HasReadRights = False
                        Me.HasNoRights = True
                    Case Else
                        CoreMessageHandler(message:="access right not implemented", arg1:=value, subname:="Group.AccessRight", messagetype:=otCoreMessageType.InternalError)

                End Select

            End Set
        End Property
#End Region

        ''' <summary>
        ''' Returns a collection of all objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of Group)
            Return ormDataObject.AllDataObject(Of Group)(orderby:=ConstFNGroupname)
        End Function

        ''' <summary>
        ''' Retrieve a User Definition
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal groupname As String, Optional domainid As String = "", Optional forcereload As Boolean = False) As Group
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return Retrieve(Of Group)(pkArray:={groupname, domainid}, domainID:=domainid, forceReload:=forcereload)
        End Function

        ''' <summary>
        ''' Create persistency for this object
        ''' </summary>
        ''' <param name="groupname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal groupname As String, Optional domainid As String = "") As Group
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim primarykey() As Object = {groupname, domainid}
            Return ormDataObject.CreateDataObject(Of Group)(primarykey, domainID:=domainid, checkUnique:=True)
        End Function

    End Class

    ''' <summary>
    ''' Group Member Definition Class of an OnTrack User
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=GroupMember.ConstObjectID, description:="group member definition of n:m relation from user to groups", _
        modulename:=ConstModuleCore, Version:=1, usecache:=True, isbootstrap:=False, adddomainbehavior:=True, adddeletefieldbehavior:=True)> _
    Public Class GroupMember
        Inherits ormDataObject


        '*** Object ID
        Public Const ConstObjectID = "GroupMember"

        '*** Schema Table
        <ormSchemaTable(version:=1)> Public Const ConstTableID As String = "tblDefGroupMembers"
        <ormSchemaIndex(columnname1:=ConstFNUsername, columnname2:=ConstFNDomainID, columnname3:=ConstFNGroupname)> Public Const ConstIndUser As String = "indUser"

        '*** Primary Keys
        <ormObjectEntry(referenceObjectEntry:=Group.ConstObjectID & "." & Group.ConstFNGroupname, primarykeyordinal:=1, _
          XID:="G1", title:="Group", description:="name of the OnTrack user group")> _
        Public Const ConstFNGroupname = "groupname"
        <ormObjectEntry(referenceObjectEntry:=User.ConstObjectID & "." & User.ConstFNUsername, primarykeyordinal:=2, _
          XID:="G1", title:="Group", description:="name of the OnTrack user group", useforeignkey:=otForeignKeyImplementation.NativeDatabase)> _
        Public Const ConstFNUsername = "username"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=3, _
                       defaultvalue:=ConstGlobalDomain, useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormSchemaForeignKey(entrynames:={ConstFNGroupname, ConstFNDomainID}, _
            foreignkeyreferences:={Group.ConstObjectID & "." & Group.ConstFNGroupname, Group.ConstObjectID & "." & Group.ConstFNDomainID}, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFKGroups = "FKGroups"


        '*** Fields


        'mapping
        <ormEntryMapping(EntryName:=ConstFNGroupname)> Private _groupname As String
        <ormEntryMapping(EntryName:=ConstFNUsername)> Private _username As String


#Region "Properties"

        ReadOnly Property GroupName() As String
            Get
                GroupName = _groupname
            End Get
        End Property
        ReadOnly Property Username() As String
            Get
                Username = _username
            End Get
        End Property

#End Region

        ''' <summary>
        ''' Returns a collection of all objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of Group)
            Return ormDataObject.AllDataObject(Of Group)(orderby:=ConstFNGroupname)
        End Function

        ''' <summary>
        ''' Retrieve a User Definition
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal groupname As String, ByVal username As String, Optional ByVal domainid As String = "", Optional forcereload As Boolean = False) As GroupMember
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return Retrieve(Of GroupMember)(pkArray:={groupname, username, domainid}, domainID:=domainid, forceReload:=forcereload)
        End Function

        ''' <summary>
        ''' Returns the Groupdefinition
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetGroup() As Group
            If Me.IsAlive(subname:="GetGroup") Then
                Return Group.Retrieve(groupname:=Me.GroupName)
            End If
        End Function
        ''' <summary>
        ''' Returns the Userdefinition
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetUser() As User
            If Me.IsAlive(subname:="GetUser") Then
                Return User.Retrieve(username:=Me.Username)
            End If
        End Function
        ''' <summary>
        ''' Create persistency for this object
        ''' </summary>
        ''' <param name="groupname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal groupname As String, ByVal username As String, Optional ByVal domainid As String = "", Optional runtimeOnly As Boolean = False) As GroupMember
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim primarykey() As Object = {groupname, username, domainid}
            Return ormDataObject.CreateDataObject(Of GroupMember)(primarykey, domainID:=domainid, checkUnique:=False, runtimeOnly:=runtimeOnly)
        End Function

    End Class

    ''' <summary>
    ''' User Definition Class of an OnTrack User
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=User.ConstObjectID, description:="user definition for OnTrack login users", _
        modulename:=ConstModuleCore, Version:=1, isbootstrap:=True, usecache:=True, adddeletefieldbehavior:=True)> _
    Public Class User
        Inherits ormDataObject
        Implements iormCloneable
        Implements iormInfusable

        '*** Object ID
        Public Const ConstObjectID = "User"
        '*** Schema Table
        <ormSchemaTable(version:=2, usecache:=True)> Public Const ConstTableID As String = "tblDefUsers"

        '*** Primary Keys
        <ormObjectEntry(typeid:=otDataType.Text, size:=50, primarykeyordinal:=1, _
            properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty},
          XID:="U1", title:="username", description:="name of the OnTrack user")> Public Const ConstFNUsername = "username"

        '*** Fields
        <ormObjectEntry(typeid:=otDataType.Text, size:=20, properties:={ObjectEntryProperty.Encrypted}, _
            validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
           XID:="U2", title:="password", description:="password of the OnTrack user")> Public Const ConstFNPassword = "password"

        <ormObjectEntry(referenceobjectentry:=Person.ConstObjectID & "." & Person.constFNID, isnullable:=True, _
         XID:="U4", aliases:={"p1"})> Public Const ConstFNPerson = "person"

        <ormObjectEntry(typeid:=otDataType.Text, size:=255, _
        XID:="U5", title:="description", description:="description of the OnTrack user")> Public Const ConstFNDescription = "desc"
        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, _
            XID:="U6", title:="is anonymous", description:="is user an anonymous user")> Public Const ConstFNIsAnonymous = "isanon"

        <ormObjectEntry(referenceobjectentry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, isnullable:=True, _
            XID:="U10", title:="Default Workspace", description:="default workspace of the OnTrack user")> Public Const ConstFNDefaultWorkspace = "defws"

        <ormObjectEntry(referenceobjectentry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, isnullable:=True, _
            XID:="U10", title:="Default Domain", description:="default domain of the OnTrack user")> Public Const ConstFNDefaultDomainID = "defdomain"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, _
           XID:="UR1", title:="Alter Schema Right", description:="has user the right to alter the database schema")> _
        Public Const ConstFNAlterSchema = "alterschema"
        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, _
          XID:="UR2", title:="Update Data Right", description:="has user the right to update data (new/change/delete)")> _
        Public Const ConstFNUpdateData = "updatedata"
        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=True, dbdefaultvalue:="1", _
          XID:="UR3", title:="Read Data Right", description:="has user the right to read the database data")> Public Const ConstFNReadData = "readdata"
        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, _
          XID:="UR4", title:="No Access", description:="has user no access")> Public Const ConstFNNoAccess = "noright"

        'overwrite the Domain ID makes no sense
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            defaultvalue:=ConstGlobalDomain, dbdefaultvalue:=ConstGlobalDomain, _
            useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '** relations
        '* Members
        <ormSchemaRelation(cascadeOnDelete:=True, cascadeOnUpdate:=True, cascadeOnCreate:=True, _
            FromEntries:={ConstFNUsername}, toEntries:={GroupMember.ConstFNUsername}, _
            LinkObject:=GetType(GroupMember))> Public Const ConstRelMembers = "members"

        'fields
        <ormEntryMapping(EntryName:=ConstFNUsername)> Private _username As String
        <ormEntryMapping(EntryName:=ConstFNPassword)> Private _password As String
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _desc As String
        <ormEntryMapping(EntryName:=ConstFNPerson)> Private _personID As String

        <ormEntryMapping(EntryName:=ConstFNDefaultWorkspace)> Private _DefaultWorkspace As String
        <ormEntryMapping(EntryName:=ConstFNDefaultDomainID)> Private _DefaultDomainID As String
        <ormEntryMapping(EntryName:=ConstFNIsAnonymous)> Private _isAnonymous As Boolean
        <ormEntryMapping(EntryName:=ConstFNReadData)> Private _hasRead As Boolean
        <ormEntryMapping(EntryName:=ConstFNUpdateData)> Private _hasUpdate As Boolean
        <ormEntryMapping(EntryName:=ConstFNAlterSchema)> Private _hasAlterSchema As Boolean
        <ormEntryMapping(EntryName:=ConstFNNoAccess)> Private _hasNoRights As Boolean

        <ormEntryMapping(Relationname:=ConstRelMembers, infusemode:=otInfuseMode.OnDemand)> Private _groupmembers As New List(Of GroupMember)
        ' dynamics
        Private _settings As New Dictionary(Of String, UserSetting)
        Private _SettingsLoaded As Boolean = False



#Region "Properties"

        Public Property Description() As String
            Get
                Description = _desc
            End Get
            Set(ByVal avalue As String)
                SetValue(entryname:=ConstFNDescription, value:=avalue)
            End Set
        End Property
        ''' <summary>
        ''' returns a list of groups
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property GroupNames() As IEnumerable(Of String)
            Get
                '* infuse the groupmembers
                If (_groupmembers Is Nothing OrElse _groupmembers.Count = 0) And Not CurrentSession.IsBootstrappingInstallationRequested Then
                    MyBase.InfuseRelation(id:=ConstRelMembers)
                End If
                Dim alist As New List(Of String)
                For Each member In _groupmembers
                    If member.IsAlive AndAlso Not alist.Contains(member.GroupName) Then alist.Add(member.GroupName)
                Next
                Return alist
            End Get
            Set(ByVal value As IEnumerable(Of String))
                For Each groupname In value
                    If _groupmembers.FindIndex(Function(x)
                                                   Return x.GroupName = groupname
                                               End Function) < 0 Then
                        Dim aGroupMember As GroupMember = _
                            GroupMember.Create(groupname:=groupname, username:=Me.Username, runtimeOnly:=CurrentSession.IsBootstrappingInstallationRequested)
                        If aGroupMember IsNot Nothing Then _groupmembers.Add(aGroupMember)
                    End If
                Next

            End Set
        End Property
        ''' <summary>
        ''' set or return the default workspace
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DefaultWorkspaceID As String

            Get
                DefaultWorkspaceID = _DefaultWorkspace
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNDefaultWorkspace, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' set or return the default workspace
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DefaultDomainID As String

            Get
                DefaultDomainID = _DefaultDomainID
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNDefaultDomainID, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Password
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Password() As String
            Get
                Password = _password
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNPassword, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets or set the person id
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PersonName() As String
            Get
                PersonName = _personID
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNPerson, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets the ontrack username
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Username() As String
            Get
                Username = _username
            End Get
        End Property
        ''' <summary>
        ''' has no rights at all ?! -> Blocked ?!
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property HasNoRights() As Boolean
            Get
                HasNoRights = _hasNoRights
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNNoAccess, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' has right to read
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property HasReadRights() As Boolean
            Get
                HasReadRights = _hasRead
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNReadData, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' has right to update and read data
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property HasUpdateRights() As Boolean
            Get
                HasUpdateRights = _hasUpdate
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNUpdateData, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Has Right to update, read and alter schema data
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property HasAlterSchemaRights() As Boolean
            Get
                HasAlterSchemaRights = _hasAlterSchema
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNAlterSchema, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' is anonymous user
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsAnonymous() As Boolean
            Get
                Return _isAnonymous
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNIsAnonymous, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' gets the accessright out of the setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property AccessRight As otAccessRight
            Get
                '* highes right first
                If Me.HasAlterSchemaRights Then
                    Return otAccessRight.AlterSchema
                ElseIf Me.HasUpdateRights Then
                    Return otAccessRight.ReadUpdateData
                ElseIf Me.HasReadRights Then
                    Return otAccessRight.ReadOnly
                End If

                Return otAccessRight.Prohibited
            End Get
            Set(value As otAccessRight)
                Select Case value
                    Case otAccessRight.AlterSchema
                        Me.HasAlterSchemaRights = True
                        Me.HasUpdateRights = True
                        Me.HasReadRights = True
                        Me.HasNoRights = False
                    Case otAccessRight.ReadUpdateData
                        Me.HasAlterSchemaRights = False
                        Me.HasUpdateRights = True
                        Me.HasReadRights = True
                        Me.HasNoRights = False
                    Case otAccessRight.ReadOnly
                        Me.HasAlterSchemaRights = False
                        Me.HasUpdateRights = False
                        Me.HasReadRights = True
                        Me.HasNoRights = False
                    Case otAccessRight.Prohibited
                        Me.HasAlterSchemaRights = False
                        Me.HasUpdateRights = False
                        Me.HasReadRights = False
                        Me.HasNoRights = True
                    Case Else
                        CoreMessageHandler(message:="access right not implemented", arg1:=value, subname:="User.AccessRight", messagetype:=otCoreMessageType.InternalError)

                End Select

            End Set
        End Property
#End Region
        ''' <summary>
        ''' returns a SQL String to insert the Admin User in the table -> bootstrap
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetInsertInitalUserSQLString(username As String, person As String, password As String, desc As String, group As String, defaultworkspace As String) As String

            Dim aSqlString As String = String.Format("INSERT INTO {0} ", ConstTableID)
            'aSqlString &= "( [username], person, [password], [desc],  defws, isanon, alterschema, readdata, updatedata, noright, UpdatedOn, CreatedOn)"
            aSqlString &= String.Format("( [{0}], [{1}], [{2}], [{3}],  [{4}], [{5}], [{6}], [{7}], {8}, [{9}], [{10}], [{11}], [{12}])", _
                                         ConstFNUsername, ConstFNPerson, ConstFNPassword, ConstFNDescription, ConstFNDefaultWorkspace, ConstFNDefaultDomainID, _
                                         ConstFNIsAnonymous, ConstFNAlterSchema, ConstFNReadData, ConstFNUpdateData, ConstFNNoAccess, ConstFNCreatedOn, ConstFNUpdatedOn)
            aSqlString &= String.Format("VALUES ('{0}','{1}', '{2}', '{3}',  '{4}','{5}', 0, 1,1,1,0, '{6}','{7}' )", _
                                        username, person, password, desc, defaultworkspace, _
                                        ConstGlobalDomain, Date.Now.ToString("yyyyMMdd hh:mm:ss"), Date.Now.ToString("yyyyMMdd hh:mm:ss"))
            Return aSqlString

        End Function

        ''' <summary>
        ''' returns a SQL String to create the table on bootstrapping
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetCreateSqlString() As String

            Dim aSqlString As String = String.Format("CREATE TABLE {0} ", ConstTableID)
            aSqlString &= String.Format("( [{0}] nvarchar(50) not null, [{1}] nvarchar(50) not null, [{2}] nvarchar(50) not null, ", _
                                        ConstFNUsername, ConstFNPassword, ConstFNPerson)
            aSqlString &= String.Format("[{0}] nvarchar(max) not null default '', [{1}] nvarchar(max) not null default '', [{2}] bit not null default 0, [{3}] bit not null default 0, [{4}] bit not null default 0, [{5}] bit not null default 0, ", _
                                        ConstFNDefaultWorkspace, ConstFNDefaultDomainID, ConstFNAlterSchema, ConstFNUpdateData, ConstFNReadData, ConstFNNoAccess)
            aSqlString &= String.Format(" [{0}] nvarchar(max) not null default '', [{1}] DATETIME not null , [{2}] Datetime not null , " & _
                                                "CONSTRAINT [{3}_primarykey] PRIMARY KEY NONCLUSTERED ([{3}] Asc) ", _
                                                ConstFNDescription, ConstFNUpdatedOn, ConstFNCreatedOn, ConstFNUsername, ConstTableID)
            aSqlString &= "WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY];"

            Return aSqlString
            '*** LEGACY working
            'Return "create table " & Me.TableID & _
            '                 " ( username nvarchar(50) not null, [password] nvarchar(50) not null, [person] nvarchar(50) not null, [group] nvarchar(50) not null, " & _
            '                 "defws nvarchar(max) not null default '', " & _
            '                 "isanon bit not null default 0, alterschema bit not null default 0, updatedata bit not null default 0, noright bit not null default 0," & _
            '                 "readdata bit not null default 1," & _
            '                 " [desc] nvarchar(max) not null default '', UpdatedOn DATETIME not null , CreatedOn Datetime not null , " & _
            '                 "CONSTRAINT [tblDefUsers_primarykey] PRIMARY KEY NONCLUSTERED ([username] Asc) " & _
            '                 "WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]" & _
            '                 ") ON [PRIMARY];"
        End Function
        ''' <summary>
        ''' Returns a collection of all objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of User)
            Return ormDataObject.AllDataObject(Of User)(orderby:=ConstFNUsername)
        End Function

        '****** getAnonymous: "static" function to return the first Anonymous user
        '******
        ''' <summary>
        ''' returns the anonyous user ( first descending username)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetAnonymous() As OnTrack.Commons.User
            Dim aObjectCollection As List(Of User)
            If CurrentSession.CurrentDBDriver.DatabaseType = otDBServerType.SQLServer Then
                aObjectCollection = ormDataObject.AllDataObject(Of User)(orderby:=ConstFNUsername, where:=ConstFNIsAnonymous & "=1")
            Else
                aObjectCollection = ormDataObject.AllDataObject(Of User)(orderby:=ConstFNUsername, where:=ConstFNIsAnonymous & "=true")
            End If

            If aObjectCollection.Count = 0 Then
                Return Nothing
            Else
                Return aObjectCollection.Item(1)
            End If

        End Function

        ''' <summary>
        ''' Retrieve a User Definition
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal username As String, Optional forcereload As Boolean = False) As User
            Return Retrieve(Of User)(pkArray:={username}, forceReload:=forcereload)
        End Function
        ''' <summary>
        ''' Returns a list of groupdefinition this belongs to
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetGroups() As List(Of Group)
            If Not Me.IsAlive(subname:="getgroup") Then Return New List(Of Group)
            Dim alist As New List(Of Group)
            '* infuse the groupmembers
            If (_groupmembers Is Nothing OrElse _groupmembers.Count = 0) And Not CurrentSession.IsBootstrappingInstallationRequested Then
                MyBase.InfuseRelation(id:=ConstRelMembers)
            End If
            '' add all the group definitions
            For Each member In _groupmembers
                If alist.FindIndex(Function(x)
                                       Return x.GroupName = member.GroupName
                                   End Function) < 0 Then
                    Dim aGroup As Group = member.GetGroup
                    If aGroup IsNot Nothing Then alist.Add(aGroup)
                End If
            Next
            Return alist
        End Function
        ''' <summary>
        ''' returns true if the setting exists
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasSetting(id As String) As Boolean
            LoadSettings() ' load since we might no have it during bootstrap
            Return _settings.ContainsKey(key:=id)
        End Function
        ''' <summary>
        ''' returns the setting valid in the domain
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSetting(id As String) As Object
            LoadSettings()
            If Me.HasSetting(id:=id) Then
                Return _settings.Item(key:=id)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' sets the setting valid in the domain
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetSetting(id As String, datatype As otDataType, value As Object) As Boolean
            Dim aSetting As New UserSetting
            LoadSettings()
            If Me.HasSetting(id:=id) Then
                aSetting = Me.GetSetting(id:=id)
            Else
                If Not aSetting.Create(Username:=Me.Username, id:=id) Then
                    aSetting = UserSetting.Retrieve(Username:=Me.Username, id:=id)
                End If
            End If

            If aSetting Is Nothing OrElse Not (aSetting.IsLoaded Or aSetting.IsCreated) Then
                Return False
            End If
            aSetting.Datatype = datatype
            aSetting.Value = value

            If Not Me.HasSetting(id:=id) Then _settings.Add(key:=id, value:=aSetting)
            Return True
        End Function
        ''' <summary>
        ''' Load the settings to the settings dictionary
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadSettings(Optional force As Boolean = False) As Boolean

            If _SettingsLoaded And Not force Then Return True

            Dim aListDomain As List(Of UserSetting) = UserSetting.RetrieveByUsername(Username:=Me.Username)

            '** overwrite
            For Each aSetting In aListDomain
                If _settings.ContainsKey(key:=aSetting.ID) Then
                    _settings.Remove(key:=aSetting.ID)
                End If
                _settings.Add(key:=aSetting.ID, value:=aSetting)
            Next

            _SettingsLoaded = False
            Return True
        End Function

        ''' <summary>
        ''' create the persistency schema with use of database driver
        ''' ATTENTION ! This can only be called if database is set up
        ''' user createSql function otherwise
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of User)(silent:=silent)
        End Function

        ''' <summary>
        ''' Create persistency for this object
        ''' </summary>
        ''' <param name="username"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal username As String) As User
            Dim primarykey() As Object = {username}
            Return ormDataObject.CreateDataObject(Of User)(primarykey, checkUnique:=True)
        End Function

    End Class
    ''' <summary>
    ''' User Setting Definition Class
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=UserSetting.ConstObjectID, description:="properties per user", _
        modulename:=ConstModuleCore, Version:=1, useCache:=True)> Public Class UserSetting
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "UserSetting"
        '** Table Schema
        <ormSchemaTableAttribute(adddeletefieldbehavior:=True, Version:=1)> Public Const ConstTableID As String = "tblDefUserSettings"

        '** Primary Key
        <ormObjectEntry(XID:="US1", referenceobjectentry:=User.ConstObjectID & "." & User.ConstFNUsername, primaryKeyordinal:=1)> _
        Const ConstFNUsername As String = User.ConstFNUsername

        '** Fields
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, primaryKeyordinal:=2, _
                    validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
           XID:="US2", title:="Setting", description:="ID of the setting per user")> Const ConstFNSettingID = "id"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, _
            XID:="US3", title:="Description")> Const ConstFNDescription = "desc"

        <ormObjectEntry(typeid:=otDataType.Memo, _
           XID:="US4", title:="value", description:="value of the user setting in string presentation")> Const ConstFNValue = "value"

        <ormObjectEntry(typeid:=otDataType.Long, defaultvalue:=otDataType.Text, _
          XID:="US5", title:="datatype", description:="data type of the user setting value")> Const ConstFNDatatype = "datatype"

        ' fields
        <ormEntryMapping(EntryName:=ConstFNUsername)> Private _Username As String = ""
        <ormEntryMapping(EntryName:=ConstFNSettingID)> Private _ID As String = ""
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNValue)> Private _valuestring As String = ""
        <ormEntryMapping(EntryName:=ConstFNDatatype)> Private _datatype As otDataType = 0
        ''' <summary>
        ''' constructor of a clsOTDBDefWorkspace
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub

#Region "Properties"
        ''' <summary>
        ''' gets the ID of the Domain
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Username() As String
            Get
                Username = _Username
            End Get

        End Property
        ''' <summary>
        ''' gets the ID of the Setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ID() As String
            Get
                ID = _ID
            End Get

        End Property
        ''' <summary>
        ''' Description of the setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                _description = value
                Me.IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' returns the datatype 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Datatype As otDataType
            Set(value As otDataType)
                _datatype = value
            End Set
            Get
                Return _datatype
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the value of the domain setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Value As Object
            Set(value As Object)
                If value Is Nothing Then
                    _valuestring = ""
                Else
                    _valuestring = value.ToString
                End If
            End Set
            Get
                Try
                    Select Case _datatype
                        Case otDataType.Binary
                            Return CBool(_valuestring)
                        Case otDataType.Date, otDataType.Time, otDataType.Timestamp
                            If _valuestring Is Nothing Then Return constNullDate
                            If IsDate(_valuestring) Then Return CDate(_valuestring)
                            If _valuestring = constNullDate.ToString OrElse _valuestring = ConstNullTime.ToString Then Return constNullDate
                            If _valuestring = "" Then Return constNullDate
                        Case otDataType.List, otDataType.Memo, otDataType.Text
                            If _valuestring Is Nothing Then Return ""
                            Return CStr(_valuestring)
                        Case otDataType.Numeric
                            If IsNumeric(_valuestring) Then
                                Return CDbl(_valuestring)
                            Else
                                Return CDbl(0)
                            End If
                        Case otDataType.Long
                            If IsNumeric(_valuestring) Then
                                Return CDbl(_valuestring)
                            Else
                                Return CDbl(0)
                            End If
                            Return CLng(_valuestring)
                        Case Else
                            CoreMessageHandler(message:="data type not covered: " & _datatype, arg1:=_valuestring, subname:="DomainSetting.value", _
                                               messagetype:=otCoreMessageType.ApplicationError)
                            Return Nothing

                    End Select
                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, message:="could not convert value to data type " & _datatype, _
                                       arg1:=_valuestring, subname:="DomainSetting.value", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End Try

            End Get
        End Property
#End Region



        ''' <summary>
        ''' Retrieve the workspaceID Cache Object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal Username As String, ByVal id As String, Optional forcereload As Boolean = False) As UserSetting
            Dim pkarray() As Object = {UCase(Username), UCase(id)}
            Return Retrieve(Of UserSetting)(pkArray:=pkarray, forceReload:=forcereload)
        End Function
        ''' <summary>
        ''' Retrieve the workspaceID Cache Object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function RetrieveByUsername(ByVal Username As String, Optional forcereload As Boolean = False) As List(Of UserSetting)
            Dim aParameterslist As New List(Of ormSqlCommandParameter)
            aParameterslist.Add(New ormSqlCommandParameter(ID:="@Username", columnname:=ConstFNUsername, value:=Username))

            Dim aList As List(Of UserSetting) = ormDataObject.AllDataObject(Of UserSetting)(ID:="allby" & Username, where:=ConstFNUsername & "= @Username", _
                                                                                      parameters:=aParameterslist)
            Return aList
        End Function
        ''' <summary>
        ''' load and infuse the current workspaceID object
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(ByVal Username As String, ByVal id As String) As Boolean
            Dim primarykey() As Object = {UCase(Trim(Username)), UCase(id)}
            Return MyBase.Inject(primarykey)
        End Function
        ''' <summary>
        ''' create the objects persistence schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of UserSetting)(silent:=silent)
        End Function

        ''' <summary>
        ''' creates with this object a new persistable Def workspaceID
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal Username As String, ByVal id As String) As Boolean
            Dim primarykey() As Object = {UCase(Username), UCase(id)}
            If MyBase.Create(primarykey, checkUnique:=False) Then
                _Username = UCase(Username)
                _ID = UCase(id)
                Return True
            Else
                Return False
            End If
        End Function

    End Class


    ''' <summary>
    ''' the person definition class
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=Person.ConstObjectID, modulename:=ConstModuleCore, description:="person definition", _
        usecache:=True, Version:=1, addDomainBehavior:=True, adddeletefieldbehavior:=True)> Public Class Person
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        '** Object ID
        Public Const ConstObjectID = "Person"
        '** Table
        <ormSchemaTable(version:=2, usecache:=True)> Public Const constTableID As String = "tblDefPersons"

        '** primary keys
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, primarykeyordinal:=1, _
            properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty},
            XID:="P1", title:="ID", description:="ID of the person")> Public Const constFNID = "id"
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=2 _
         , useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '** fields
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, _
          XID:="P2", title:="First Name", description:="first name of the person")> Public Const constFNFirstName = "firstname"
        <ormObjectEntry(typeid:=otDataType.Text, size:=255, _
         XID:="P3", title:="Middle Names", description:="mid names of the person")> Public Const constFNMidNames = "midnames"
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, _
         XID:="P4", title:="Sir Name", description:="sir name of the person")> Public Const constFNSirName = "sirname"
        <ormObjectEntry(typeid:=otDataType.Memo, _
           XID:="P5", title:="Description", description:="description of the person")> Public Const constFNDescription = "desc"
        <ormObjectEntry(typeid:=otDataType.Bool, _
           XID:="P6", title:="Role", description:="set if the person is a role")> Public Const ConstFNIsRole = "isrole"
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, _
        XID:="P13", title:="Company Name", description:="name of the persons company")> Public Const constFNCompany = "company"
        <ormObjectEntry(referenceObjectEntry:=ConstObjectID & "." & constFNID, XID:="P7", Title:="superior ID", description:="ID of the superior manager")> _
        Public Const ConstFNManager = "superid"
        <ormObjectEntry(referenceObjectEntry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, _
            XID:="P8")> Public Const ConstFNOrgUnit = "orgunit"
        <ormObjectEntry(typeid:=otDataType.Text, size:=255, _
          XID:="P9", title:="eMail", description:="eMail Address of the person")> Public Const constFNeMail = "email"
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, _
         XID:="P10", title:="phone", description:="phone of the person")> Public Const constFNPhone = "phone"
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, _
         XID:="P11", title:="phone", description:="mobile of the person")> Public Const constFNMobile = "mobile"
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, _
         XID:="P12", title:="phone", description:="fax of the person")> Public Const constFNFax = "fax"

        ' field mapping
        <ormEntryMapping(EntryName:=constFNID)> Private _id As String = ""
        <ormEntryMapping(EntryName:=constFNFirstName)> Private _firstname As String = ""
        <ormEntryMapping(EntryName:=constFNMidNames)> Private _midnames As String = ""
        <ormEntryMapping(EntryName:=constFNSirName)> Private _sirname As String = ""
        <ormEntryMapping(EntryName:=ConstFNIsRole)> Private _isrole As Boolean = False
        <ormEntryMapping(EntryName:=constFNDescription)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNManager)> Private _managerid As String = ""
        <ormEntryMapping(EntryName:=ConstFNOrgUnit)> Private _orgunitID As String = ""
        <ormEntryMapping(EntryName:=constFNCompany)> Private _companyID As String = ""
        <ormEntryMapping(EntryName:=constFNeMail)> Private _emailaddy As String = ""
        <ormEntryMapping(EntryName:=constFNPhone)> Private _phone As String = ""
        <ormEntryMapping(EntryName:=constFNMobile)> Private _mobile As String = ""
        <ormEntryMapping(EntryName:=constFNFax)> Private _fax As String = ""

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New(constTableID)
        End Sub

#Region "Properties"
        ''' <summary>
        ''' returns the ID of the Person
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ID() As String
            Get
                ID = _id
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the firstname.
        ''' </summary>
        ''' <value>The firstname.</value>
        Public Property Firstname() As String
            Get
                Return Me._firstname
            End Get
            Set(value As String)
                If _firstname.ToLower <> value.ToLower Then
                    Dim pattern As String = "\b(\w|['-])+\b"
                    ' With lambda support:
                    Dim result As String = Regex.Replace(value.ToLower, pattern, _
                        Function(m) m.Value(0).ToString().ToUpper() & m.Value.Substring(1))
                    Me._firstname = result
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the midnames
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Midnames() As String()
            Get
                Return Converter.String2Array(_midnames)
            End Get
            Set(avalue As String())
                If Not Array.Equals(avalue, _midnames) Then
                    Dim pattern As String = "\b(\w|['-])+\b"
                    ' With lambda support:
                    Dim result As String = Regex.Replace(LCase(Converter.Array2String(avalue)), pattern, _
                        Function(m) m.Value(0).ToString().ToUpper() & m.Value.Substring(1))
                    _midnames = result
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the Sirname.
        ''' </summary>
        ''' <value>The sirname.</value>
        Public Property Sirname() As String
            Get
                Return Me._sirname
            End Get
            Set(value As String)
                If _sirname.ToLower <> value.ToLower Then
                    Dim pattern As String = "\b(\w|['-])+\b"
                    ' With lambda support:
                    Dim result As String = Regex.Replace(value.ToLower, pattern, _
                        Function(m) m.Value(0).ToString().ToUpper() & m.Value.Substring(1))
                    _sirname = result
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns the description of the person
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                _description = value
                IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the role flag
        ''' </summary>
        ''' <value></value>
        Public Property IsRole() As Boolean
            Get
                Return Me._isrole
            End Get
            Set(value As Boolean)
                If _isrole <> value Then
                    Me._isrole = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the company ID.
        ''' </summary>
        ''' <value>The company name.</value>
        Public Property Company() As String
            Get
                Return Me._companyID
            End Get
            Set(value As String)
                If _companyID.ToLower <> value.ToLower Then
                    Me._companyID = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the ManagerID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ManagerID() As String
            Get
                ManagerID = _managerid
            End Get
            Set(value As String)
                If ManagerID.ToLower <> value.ToLower Then
                    _managerid = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the Organization Unit ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Property OrgUnitID() As String
            Get
                OrgUnitID = _orgunitID
            End Get
            Set(value As String)
                If _orgunitID.ToLower <> value.ToLower Then
                    _orgunitID = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the Organization Unit 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Property OrgUnit() As OrgUnit
            Get
                Return OrgUnit.Retrieve(id:=_orgunitID)
            End Get
            Set(value As OrgUnit)
                If _orgunitID.ToLower <> value.ID.ToLower Then
                    _orgunitID = value.ID
                    IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the email address 
        ''' </summary>
        ''' <value>The company name.</value>
        Public Property eMail() As String
            Get
                Return Me._emailaddy
            End Get
            Set(value As String)
                If _emailaddy.ToLower <> value.ToLower Then
                    Me._emailaddy = LCase(Trim(value))
                    IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the Phone number 
        ''' </summary>
        ''' <value>The company name.</value>
        Public Property Phone() As String
            Get
                Return Me._phone
            End Get
            Set(value As String)
                If _phone.ToLower <> value.ToLower Then
                    Me._phone = LCase(Trim(value))
                    IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the email address 
        ''' </summary>
        ''' <value>The company name.</value>
        Public Property Fax() As String
            Get
                Return Me._fax
            End Get
            Set(value As String)
                If _fax.ToLower <> value.ToLower Then
                    Me._fax = LCase(Trim(value))
                    IsChanged = True
                End If
            End Set
        End Property
#End Region
        ''' <summary>
        ''' loads the persistence object with ID from the parameters
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal firstname As String, ByVal midnames As String(), ByVal sirname As String, Optional domainID As String = "") As Person
            Return Retrieve(id:=BuildID(firstname:=firstname, midnames:=midnames, sirname:=sirname), domainID:=domainID)
        End Function
        ''' <summary>
        ''' Retrieve
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal id As String, Optional domainID As String = "", Optional forcereload As Boolean = False) As Person
            Dim primarykey() As Object = {id, domainID}
            Return Retrieve(Of Person)(pkArray:=primarykey, domainID:=domainID, forceReload:=forcereload)
        End Function
        ''' <summary>
        ''' loads the persistence object with ID from the parameters
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Inject(ByVal firstname As String, ByVal sirname As String, Optional ByVal midnames As String() = Nothing, Optional domainID As String = "") As Boolean
            Return Inject(id:=BuildID(firstname:=firstname, midnames:=midnames, sirname:=sirname), domainID:=domainID)
        End Function
        ''' <summary>
        ''' Load and infuses a object by primary key
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Inject(ByVal id As String, Optional domainID As String = "") As Boolean
            Dim primarykey() As Object = {id, domainID}
            Return MyBase.Inject(pkArray:=primarykey, domainID:=domainID)
        End Function
        ''' <summary>
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of Person)(silent:=silent)

        End Function

        ''' <summary>
        ''' returns a collection of all Person Definition Objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All(Optional domainID As String = "") As List(Of Person)
            Return ormDataObject.AllDataObject(Of Person)(domainID:=domainID)
        End Function

        ''' <summary>
        ''' build the ID string out of the names
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function BuildID(ByVal firstname As String, ByVal sirname As String, Optional ByVal midnames As String() = Nothing) As String
            Dim pattern As String = "\b(\w|['-])+\b"
            Dim midnamesS As String = ""
            ' With lambda support:
            firstname = Regex.Replace(firstname.ToLower, pattern, Function(m) m.Value(0).ToString().ToUpper() & m.Value.Substring(1))
            sirname = Regex.Replace(firstname.ToLower, pattern, Function(m) m.Value(0).ToString().ToUpper() & m.Value.Substring(1))
            If midnames IsNot Nothing Then midnamesS = Regex.Replace(LCase(Converter.Array2String(midnames)), pattern, Function(m) m.Value(0).ToString().ToUpper() & m.Value.Substring(1))

            If midnamesS <> "" Then
                Return sirname & ", " & firstname & " (" & midnamesS & ")"
            Else
                Return sirname & ", " & firstname
            End If
        End Function
        ''' <summary>
        ''' Creates the persistence object
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal id As String, Optional domainID As String = "") As Boolean
            Dim primarykey() As Object = {id, domainID}
            ' set the primaryKey
            Return MyBase.Create(primarykey, domainID:=domainID, checkUnique:=True)
        End Function
        ''' <summary>
        ''' Creates the persistence object with ID from the parameters
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal firstname As String, ByVal sirname As String, Optional ByVal midnames As String() = Nothing, Optional domainID As String = "") As Boolean
            Return Create(id:=BuildID(firstname:=firstname, midnames:=midnames, sirname:=sirname), domainID:=domainID)
        End Function
    End Class

    '************************************************************************************
    '***** CLASS ObjectLogMessageDef describes an Error or Info Message
    '*****
    ''' <summary>
    ''' Object Message Definition Class - bound messages to a buisiness object
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(ID:=ObjectLogMessageDef.ConstObjectID, Modulename:=ConstModuleCore, Description:="message definitions for object messages")> _
    Public Class ObjectLogMessageDef
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "ObjectLogMessageDefinition"
        '* Schema Mapping
        <ormSchemaTable(version:=1, addDomainBehavior:=True)> Public Const ConstTableID As String = "tblDefObjectLogMessages"

        '* primary keys
        <ormObjectEntry(typeid:=otDataType.Text, size:=20, primarykeyordinal:=1, _
           XID:="omd1", title:="ID", description:="Identifier of the object message")> Public Const ConstFNMessageID = "msglogtag"
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=2 _
        , useforeignkey:=otForeignKeyImplementation.NativeDatabase, defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '* fields
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, _
          XID:="omd2", title:="Area", description:="area of the object message")> Public Const constFNArea = "area"
        <ormObjectEntry(typeid:=otDataType.Numeric, _
        XID:="omd3", title:="Weight", description:="weight of the object message")> Public Const constFNWeight = "weight"
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, _
        XID:="omd4", title:="Type", description:="type of the object message")> Public Const constFNType = "typeid"
        <ormObjectEntry(typeid:=otDataType.Text, size:=1024, _
        XID:="omd5", title:="Text", description:="message text of the object message")> Public Const constFNText = "message"
        <ormObjectEntry(typeid:=otDataType.Memo,
        XID:="omd6", title:="Description", description:="additional description and help text of the object message")> Public Const constFNDescription = "desc"

        <ormObjectEntry(referenceObjectEntry:=StatusItem.ConstObjectID & "." & StatusItem.constFNType, _
        XID:="omd11", isnullable:=True, title:="Status Code 1", description:="status type #1 of the object message")> Public Const constFNSType1 = "stype1"
        <ormObjectEntry(referenceObjectEntry:=StatusItem.ConstObjectID & "." & StatusItem.constFNType, _
         XID:="omd12", isnullable:=True, title:="Status Code 2", description:="status type #2 of the object message")> Public Const constFNSType2 = "stype2"
        <ormObjectEntry(referenceObjectEntry:=StatusItem.ConstObjectID & "." & StatusItem.constFNType, _
         XID:="omd13", isnullable:=True, title:="Status Code 3", description:="status type #3 of the object message")> Public Const constFNSType3 = "stype3"

        <ormObjectEntry(referenceObjectEntry:=StatusItem.ConstObjectID & "." & StatusItem.constFNCode, _
        XID:="omd21", isnullable:=True, title:="Status Code 1", description:="status code #1 of the object message")> Public Const constFNSCode1 = "scode1"
        <ormObjectEntry(referenceObjectEntry:=StatusItem.ConstObjectID & "." & StatusItem.constFNCode, _
         XID:="omd22", isnullable:=True, title:="Status Code 2", description:="status code #2 of the object message")> Public Const constFNSCode2 = "scode2"
        <ormObjectEntry(referenceObjectEntry:=StatusItem.ConstObjectID & "." & StatusItem.constFNCode, _
         XID:="omd23", isnullable:=True, title:="Status Code 3", description:="status code #3 of the object message")> Public Const constFNSCode3 = "scode3"


        ' field mapping
        <ormEntryMapping(EntryName:=ConstFNMessageID)> Private _id As Long
        <ormEntryMapping(EntryName:=constFNWeight)> Private _weight As Double
        <ormEntryMapping(EntryName:=constFNArea)> Private _area As String = ""
        Private _typeid As otAppLogMessageType '* handled by infuse event
        <ormEntryMapping(EntryName:=constFNText)> Private _message As String = ""
        <ormEntryMapping(EntryName:=constFNDescription)> Private _desc As String = ""
        <ormEntryMapping(EntryName:=constFNSCode1)> Private _status1 As String = ""
        <ormEntryMapping(EntryName:=constFNSType1)> Private _statustype1 As String = ""
        <ormEntryMapping(EntryName:=constFNSCode2)> Private _status2 As String = ""
        <ormEntryMapping(EntryName:=constFNSType2)> Private _statustype2 As String = ""
        <ormEntryMapping(EntryName:=constFNSCode3)> Private _status3 As String = ""
        <ormEntryMapping(EntryName:=constFNSType3)> Private _statustype3 As String = ""



        ''' <summary>
        ''' constructor of a Message Definition
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub

#Region "Properties"
        ReadOnly Property ID() As Long
            Get
                ID = _id
            End Get
        End Property

        Public Property Message() As String
            Get
                Message = _message
            End Get
            Set(value As String)
                _message = value
                IsChanged = True
            End Set
        End Property


        Public Property Weight() As Double
            Get
                Weight = _weight
            End Get
            Set(avalue As Double)
                If _weight <> avalue Then
                    _weight = avalue
                    IsChanged = True
                End If
            End Set
        End Property


        Public Property TypeID() As otAppLogMessageType
            Get
                TypeID = _typeid
            End Get
            Set(avalue As otAppLogMessageType)
                If _typeid <> avalue Then
                    _typeid = avalue
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property Area() As String
            Get
                Area = _area
            End Get
            Set(ByVal avalue As String)
                If _area <> avalue Then
                    _area = avalue
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property Statuscode1() As String
            Get
                Statuscode1 = _status1
            End Get
            Set(avalue As String)
                If _status1 <> avalue.ToLower Then
                    _status1 = avalue.ToLower
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property Statuscode2() As String
            Get
                Statuscode2 = _status2
            End Get
            Set(avalue As String)
                If _status2 <> avalue.ToLower Then
                    _status2 = avalue.ToLower
                    IsChanged = True
                End If
            End Set
        End Property
        Public Property Statuscode3() As String
            Get
                Statuscode3 = _status3
            End Get
            Set(avalue As String)
                If _status3 <> avalue.ToLower Then
                    _status3 = avalue.ToLower
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property Statustype1() As String
            Get
                Statustype1 = _statustype1
            End Get
            Set(avalue As String)
                If _statustype1 <> avalue.ToLower Then
                    _statustype1 = avalue.ToLower
                    IsChanged = True
                End If
            End Set
        End Property
        Public Property Statustype2() As String
            Get
                Statustype2 = _statustype2
            End Get
            Set(avalue As String)
                If _statustype2 <> avalue.ToLower Then
                    _statustype2 = avalue.ToLower
                    IsChanged = True
                End If
            End Set
        End Property
        Public Property Statustype3() As String
            Get
                Statustype3 = _statustype3
            End Get
            Set(avalue As String)
                If _statustype3 <> avalue.ToLower Then
                    _statustype3 = avalue.ToLower
                    IsChanged = True
                End If
            End Set
        End Property
#End Region

        Public Function GetStatusCodeOf(ByVal typeid As String) As String
            If Not Me.IsLoaded And Not Me.IsCreated Then
                GetStatusCodeOf = ""
                Exit Function
            End If

            If typeid.ToLower = Me.Statustype1 Then
                GetStatusCodeOf = Me.Statuscode1
                Exit Function
            ElseIf typeid.ToLower = Me.Statustype2 Then
                GetStatusCodeOf = Me.Statuscode2
                Exit Function
            ElseIf typeid.ToLower = Me.Statustype3 Then
                GetStatusCodeOf = Me.Statuscode2
                Exit Function
            Else
                GetStatusCodeOf = ""
                Exit Function
            End If
        End Function

        Public Function GetMessageTypeID(typeid As String) As otAppLogMessageType
            Select Case typeid.ToLower
                Case OTDBConst_MessageTypeid_error.ToLower
                    GetMessageTypeID = otAppLogMessageType.[Error]
                Case OTDBConst_MessageTypeid_info.ToLower
                    GetMessageTypeID = otAppLogMessageType.Info
                Case OTDBConst_MessageTypeid_attention.ToLower
                    GetMessageTypeID = otAppLogMessageType.Attention
                Case OTDBConst_MessageTypeid_warning.ToLower
                    GetMessageTypeID = otAppLogMessageType.Warning
                Case Else
                    GetMessageTypeID = 0
            End Select
        End Function
        Public Function GetMessageTypeName(typeid As otAppLogMessageType) As String
            Select Case typeid
                Case otAppLogMessageType.[Error]
                    GetMessageTypeName = OTDBConst_MessageTypeid_error
                Case otAppLogMessageType.Info
                    GetMessageTypeName = OTDBConst_MessageTypeid_info
                Case otAppLogMessageType.Attention
                    GetMessageTypeName = OTDBConst_MessageTypeid_attention
                Case otAppLogMessageType.Warning
                    GetMessageTypeName = OTDBConst_MessageTypeid_warning
            End Select
        End Function
        ''' <summary>
        ''' infuses the Log message definition by a record
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Sub OnInfused(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.ClassOnColumnMappingInfused
            Dim aVAlue As Object

            Try
                aVAlue = e.Record.GetValue(constFNType)
                Select Case aVAlue.tolower
                    Case OTDBConst_MessageTypeid_error.ToLower
                        _typeid = otAppLogMessageType.[Error]
                    Case OTDBConst_MessageTypeid_info.ToLower
                        _typeid = otAppLogMessageType.Info
                    Case OTDBConst_MessageTypeid_attention.ToLower
                        _typeid = otAppLogMessageType.Attention
                    Case OTDBConst_MessageTypeid_warning.ToLower
                        _typeid = otAppLogMessageType.Warning
                End Select

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="ObjectLogMessageDef.Infuse")
            End Try

        End Sub
        ''' <summary>
        ''' returns a Object Log Message Definition Object from the data store
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="domainID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal id As String, Optional domainID As String = "") As ObjectLogMessageDef
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim primarykey() As Object = {id, domainID}
            Return Retrieve(Of ObjectLogMessageDef)(pkArray:=primarykey)
        End Function
        ''' <summary>
        ''' Load the Log Message Definition from store
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(ByVal id As String, Optional domainID As String = "") As Boolean
            Dim primarykey() As Object = {id}
            Return MyBase.Inject(pkArray:=primarykey, domainID:=domainID)
        End Function
        ''' <summary>
        ''' create the persitency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of ObjectLogMessageDef)(silent:=silent)
            'Dim aFieldDesc As New ormFieldDescription
            'Dim PrimaryColumnNames As New Collection
            'Dim aTableDef As New ObjectDefinition

            'With aTableDef
            '    .Create(ConstTableID)
            '    .Delete()

            '    aFieldDesc.Tablename = ConstTableID
            '    aFieldDesc.ID = ""
            '    aFieldDesc.Parameter = ""
            '    aFieldDesc.Relation = New String() {}

            '    '***
            '    '*** Fields
            '    '****

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "message id"
            '    aFieldDesc.ID = "lm1"
            '    aFieldDesc.ColumnName = "id"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    PrimaryColumnNames.Add(aFieldDesc.ColumnName)

            '    'fieldnames
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "area of message"
            '    aFieldDesc.ID = "lm2"
            '    aFieldDesc.ColumnName = "area"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Numeric
            '    aFieldDesc.Title = "weight of message"
            '    aFieldDesc.ID = "lm3"
            '    aFieldDesc.ColumnName = "weight"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "typeid of message"
            '    aFieldDesc.ID = "lm4"
            '    aFieldDesc.ColumnName = "typeid"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "message"
            '    aFieldDesc.ID = "lm11"
            '    aFieldDesc.ColumnName = "msg"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Memo
            '    aFieldDesc.Title = "description"
            '    aFieldDesc.ID = "lm12"
            '    aFieldDesc.ColumnName = "desc"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '    ' STATUS 1
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "status code 1"
            '    aFieldDesc.ID = "lm5"
            '    aFieldDesc.Relation = New String() {"stat2"}
            '    aFieldDesc.ColumnName = "scode1"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "status type 1"
            '    aFieldDesc.ID = "lm6"
            '    aFieldDesc.Relation = New String() {"stat1"}
            '    aFieldDesc.ColumnName = "stype1"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    ' STATUS 2
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "status code 2"
            '    aFieldDesc.ID = "lm7"
            '    aFieldDesc.Relation = New String() {"stat2"}
            '    aFieldDesc.ColumnName = "scode2"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "status type 2"
            '    aFieldDesc.ID = "lm8"
            '    aFieldDesc.Relation = New String() {"stat1"}
            '    aFieldDesc.ColumnName = "stype2"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    ' STATUS 3
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "status code 3"
            '    aFieldDesc.ID = "lm9"
            '    aFieldDesc.Relation = New String() {"stat2"}
            '    aFieldDesc.ColumnName = "scode3"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "status type 3"
            '    aFieldDesc.ID = "lm10"
            '    aFieldDesc.Relation = New String() {"stat1"}
            '    aFieldDesc.ColumnName = "stype3"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    '***
            '    '*** TIMESTAMP
            '    '****
            '    aFieldDesc.Datatype = otFieldDataType.Timestamp
            '    aFieldDesc.Title = "last Update"
            '    aFieldDesc.ColumnName = ConstFNUpdatedOn
            '    aFieldDesc.Relation = Nothing
            '    aFieldDesc.ID = ""
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Timestamp
            '    aFieldDesc.Title = "creation Date"
            '    aFieldDesc.Relation = Nothing
            '    aFieldDesc.ColumnName = ConstFNCreatedOn
            '    aFieldDesc.ID = ""
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    ' Index
            '    Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)

            '    ' persist
            '    .Persist()
            '    ' change the database
            '    .AlterSchema()
            'End With

            'CreateSchema = True
            'Exit Function

        End Function

        ''' <summary>
        ''' Persist the Log Message Definition to the store
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Sub OnRecordFed(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.ClassOnFed

            Try
                '* transform
                Select Case DirectCast(e.DataObject, ObjectLogMessageDef).TypeID
                    Case otAppLogMessageType.[Error]
                        Call e.Record.SetValue(constFNType, "ERROR")
                    Case otAppLogMessageType.Info
                        Call e.Record.SetValue(constFNType, "INFO")
                    Case otAppLogMessageType.Attention
                        Call e.Record.SetValue(constFNType, "ATTENTION")
                    Case otAppLogMessageType.Warning
                        Call e.Record.SetValue(constFNType, "WARNING")

                End Select

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="ObjectLogMessageDef.OnRecordFed")
            End Try
        End Sub
        ''' <summary>
        ''' return all Log Message Definitions
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function All(Optional domainID As String = "") As List(Of ObjectLogMessageDef)
            Return ormDataObject.AllDataObject(Of ObjectLogMessageDef)(domainID:=domainID)
        End Function

        ''' <summary>
        ''' Create a persistable Log Message
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal id As String, Optional ByVal domainID As String = "") As Boolean
            Dim primarykey() As Object = {id}
            ' set the primaryKey
            Return MyBase.Create(primarykey, domainID:=domainID, checkUnique:=True)
        End Function
    End Class

    ''' <summary>
    ''' Status ITEM Class for Stati in Object Messages
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=StatusItem.ConstObjectID, description:="status item description for object messages and others", _
        modulename:=ConstModuleCore, Version:=1, usecache:=True, addDomainBehavior:=True, adddeletefieldbehavior:=True)> Public Class StatusItem
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable

        '** Status Item
        Public Const ConstObjectID = "StatusItem"

        '** Table
        <ormSchemaTable(version:=2, usecache:=True)> Public Const ConstTableID As String = "tblDefStatusItems"

        '* primary Key
        <ormObjectEntry(typeid:=otDataType.Text, size:=50, primarykeyordinal:=1, _
            properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty},
            XID:="si1", title:="Type", description:="type of the status")> Public Const constFNType = "typeid"

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, primarykeyordinal:=2, _
            properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty},
           XID:="si2", title:="Code", description:="code of the status")> Public Const constFNCode = "code"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=3 _
         , useforeignkey:=otForeignKeyImplementation.NativeDatabase, defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID


        '* fields
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, _
           XID:="si3", title:="Name", description:="name of the status")> Public Const constFNName = "name"
        <ormObjectEntry(typeid:=otDataType.Memo, _
          XID:="si4", title:="Description", description:="description of the status")> Public Const constFNDescription = "desc"
        <ormObjectEntry(typeid:=otDataType.Text, size:=50, _
          XID:="si5", title:="KPICode", description:="KPI code of the status")> Public Const constFNKPICode = "kpicode"
        <ormObjectEntry(typeid:=otDataType.Numeric, _
          XID:="si6", title:="Weight", description:="weight of the status")> Public Const constFNWeight = "weight"
        <ormObjectEntry(typeid:=otDataType.Bool, _
          XID:="si11", title:="Start", description:="set if the status is an start status")> Public Const constFNIsStartStatus = "isstart"
        <ormObjectEntry(typeid:=otDataType.Bool, _
          XID:="si12", title:="Intermediate", description:="set if the status is an intermediate status")> Public Const constFNIsEndStatus = "isend"
        <ormObjectEntry(typeid:=otDataType.Bool, _
         XID:="si13", title:="End", description:="set if the status is an end status")> Public Const constFNIsIntermediateStatus = "isimed"

        <ormObjectEntry(typeid:=otDataType.Long, _
          XID:="si21", title:="Foreground", description:="RGB foreground color code")> Public Const ConstFNFGColor = "fgcolor"
        <ormObjectEntry(typeid:=otDataType.Long, _
          XID:="si22", title:="Background", description:="RGB background color code")> Public Const ConstFNBGColor = "bgcolor"
        <ormObjectEntry(typeid:=otDataType.Long, _
          XID:="si23", title:="KPI Foreground", description:="RGB foreground kpi color code")> Public Const ConstFNKPIFGColor = "kpifgcolor"
        <ormObjectEntry(typeid:=otDataType.Long, _
          XID:="si24", title:="KPI Background", description:="RGB background kpi color code")> Public Const ConstFNKPIBGColor = "kpibgcolor"


        '* mappings
        <ormEntryMapping(EntryName:=constFNType)> Private _type As String = ""  ' Status Type
        <ormEntryMapping(EntryName:=constFNCode)> Private _code As String = ""  ' code
        <ormEntryMapping(EntryName:=ConstFNDomainID)> Private _DomainID As String = ""  ' code
        <ormEntryMapping(EntryName:=constFNName)> Private _name As String = ""
        <ormEntryMapping(EntryName:=constFNDescription)> Private s_descriptio As String = ""
        <ormEntryMapping(EntryName:=constFNKPICode)> Private _kpicode As String = ""
        <ormEntryMapping(EntryName:=constFNWeight)> Private _weight As Double
        <ormEntryMapping(EntryName:=ConstFNFGColor)> Private _fgcolor As Long
        <ormEntryMapping(EntryName:=ConstFNBGColor)> Private _bgcolor As Long
        <ormEntryMapping(EntryName:=ConstFNKPIFGColor)> Private _kpifgcolor As Long
        <ormEntryMapping(EntryName:=ConstFNKPIBGColor)> Private _kpibgcolor As Long
        <ormEntryMapping(EntryName:=constFNIsEndStatus)> Private _endStatus As Boolean
        <ormEntryMapping(EntryName:=constFNIsStartStatus)> Private _startStatus As Boolean
        <ormEntryMapping(EntryName:=constFNIsIntermediateStatus)> Private _intermediateStatus As Boolean

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub



#Region "Properties"

        ReadOnly Property TypeID() As String
            Get
                TypeID = _type
            End Get

        End Property
        ReadOnly Property Code() As String
            Get
                Code = _code
            End Get

        End Property
        ReadOnly Property DomainID() As String
            Get
                DomainID = _DomainID
            End Get

        End Property
        Public Property Description() As String
            Get
                Description = s_descriptio
            End Get
            Set(value As String)
                s_descriptio = value
                IsChanged = True
            End Set
        End Property


        Public Property Name() As String
            Get
                Name = _name
            End Get
            Set(value As String)
                _name = value
                IsChanged = True
            End Set
        End Property

        Public Property KPICode() As String
            Get
                KPICode = _kpicode
            End Get
            Set(value As String)
                If _kpicode.ToLower <> value.ToLower Then
                    _kpicode = value.ToLower
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property Weight() As Double
            Get
                Weight = _weight
            End Get
            Set(value As Double)
                If value <> _weight Then
                    _weight = value
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property IsStartStatus() As Boolean
            Get
                IsStartStatus = _startStatus
            End Get
            Set(value As Boolean)
                _startStatus = value
                IsChanged = True
            End Set
        End Property


        Public Property IsIntermediateStatus() As Boolean
            Get
                IsIntermediateStatus = _intermediateStatus
            End Get
            Set(value As Boolean)
                _intermediateStatus = value
                IsChanged = True
            End Set
        End Property


        Public Property IsEndStatus() As Boolean
            Get
                IsEndStatus = _endStatus
            End Get
            Set(value As Boolean)
                _endStatus = value
                IsChanged = True
            End Set
        End Property

        Public Property Formatbgcolor() As Long
            Get
                Formatbgcolor = _bgcolor
            End Get
            Set(value As Long)
                _bgcolor = value
                IsChanged = True
            End Set
        End Property



        Public Property Formatkpibgcolor() As Long
            Get
                Formatbgcolor = _kpibgcolor
            End Get
            Set(value As Long)
                _kpibgcolor = value
                IsChanged = True
            End Set
        End Property

        Public Property Formatfgcolor() As Long
            Get
                Formatfgcolor = _fgcolor
            End Get
            Set(value As Long)
                _fgcolor = value
                IsChanged = True
            End Set
        End Property


        Public Property Formatkpifgcolor() As Long
            Get
                Formatfgcolor = _fgcolor
            End Get
            Set(value As Long)
                _kpifgcolor = value
                IsChanged = True
            End Set
        End Property
#End Region


        ''' <summary>
        ''' Retrieve from datastore
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve([typeid] As String, code As String, Optional domainID As String = "", Optional forcereload As Boolean = False) As StatusItem
            Dim pkarry() As Object = {LCase([typeid]), code.ToLower, UCase(domainID)}
            Return Retrieve(Of StatusItem)(pkArray:=pkarry, domainID:=domainID, forceReload:=forcereload)
        End Function

        ''' <summary>
        ''' Load and Infuse a status item defintion
        ''' </summary>
        ''' <param name="TYPEID"></param>
        ''' <param name="code"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Inject(ByVal typeid As String, ByVal code As String, Optional domainID As String = "") As Boolean
            Dim pkarry() As Object = {typeid.ToLower, code.ToLower, UCase(domainID)}
            Return MyBase.Inject(pkArray:=pkarry, domainID:=domainID)
        End Function
        ''' <summary>
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of StatusItem)(silent:=silent)
            'Dim aFieldDesc As New ormFieldDescription
            'Dim PrimaryColumnNames As New Collection
            'Dim aStore As New ObjectDefinition

            'With aStore
            '    .Create(ConstTableID)
            '    .Delete()
            '    aFieldDesc.Tablename = ConstTableID
            '    aFieldDesc.ID = ""
            '    aFieldDesc.Parameter = ""

            '    '***
            '    '*** Fields
            '    '****

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "type id of the status"
            '    aFieldDesc.ID = "stat1"
            '    aFieldDesc.ColumnName = "typeid"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    PrimaryColumnNames.Add(aFieldDesc.ColumnName)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "code"
            '    aFieldDesc.ID = "stat2"
            '    aFieldDesc.ColumnName = "code"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    PrimaryColumnNames.Add(aFieldDesc.ColumnName)

            '    'fieldnames
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "name of status"
            '    aFieldDesc.ID = "stat3"
            '    aFieldDesc.ColumnName = "name"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "description"
            '    aFieldDesc.ID = "stat4"
            '    aFieldDesc.ColumnName = "desc"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "kpi code of this status"
            '    aFieldDesc.ID = "stat5"
            '    aFieldDesc.ColumnName = "kpicode"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "weight"
            '    aFieldDesc.ID = "stat6"
            '    aFieldDesc.ColumnName = "weight"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Bool
            '    aFieldDesc.Title = "is end status"
            '    aFieldDesc.ID = "stat7"
            '    aFieldDesc.ColumnName = "isend"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Bool
            '    aFieldDesc.Title = "is start status"
            '    aFieldDesc.ID = "stat8"
            '    aFieldDesc.ColumnName = "isstart"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Bool
            '    aFieldDesc.Title = "is intermediate status"
            '    aFieldDesc.ID = "stat9"
            '    aFieldDesc.ColumnName = "isimed"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "foreground color"
            '    aFieldDesc.ID = "stat10"
            '    aFieldDesc.ColumnName = "fgcolor"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "background color"
            '    aFieldDesc.ID = "stat11"
            '    aFieldDesc.ColumnName = "bgcolor"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "kpi code foreground color"
            '    aFieldDesc.ID = "stat12"
            '    aFieldDesc.ColumnName = "kpifgcolor"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "kpi code background color"
            '    aFieldDesc.ID = "stat13"
            '    aFieldDesc.ColumnName = "kpibgcolor"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    '***
            '    '*** TIMESTAMP
            '    '****
            '    aFieldDesc.Datatype = otFieldDataType.Timestamp
            '    aFieldDesc.Title = "last Update"
            '    aFieldDesc.ColumnName = ConstFNUpdatedOn
            '    aFieldDesc.ID = ""
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Timestamp
            '    aFieldDesc.Title = "creation Date"
            '    aFieldDesc.ColumnName = ConstFNCreatedOn
            '    aFieldDesc.ID = ""
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    ' Index
            '    Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)

            '    ' persist
            '    .Persist()
            '    ' change the database
            '    .AlterSchema()
            'End With

            ''
            'CreateSchema = True
            'Exit Function


        End Function

        ''' <summary>
        ''' create a persistable object 
        ''' </summary>
        ''' <param name="typeid"></param>
        ''' <param name="code"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal typeid As String, ByVal code As String, Optional ByVal domainID As String = "") As Boolean
            ' set the primaryKey
            Dim primarykey() As Object = {typeid.ToLower, code.ToLower, domainID}
            Return MyBase.Create(primarykey, domainID:=domainID, checkUnique:=True)
        End Function

    End Class

    ''' <summary>
    ''' Workspace Definition Class
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=Workspace.ConstObjectID, adddomainBehavior:=False, adddeletefieldbehavior:=True, usecache:=True, _
        modulename:=ConstModuleCore, description:="workspace definition for vertical grouping in scheduling", _
        Version:=1, useCache:=True)> Public Class Workspace
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "Workspace"

        '** Table Schema
        <ormSchemaTableAttribute(Version:=2, usecache:=True)> Public Const ConstTableID As String = "tblDefWorkspaces"

        '** primary Keys
        <ormObjectEntry(typeid:=otDataType.Text, size:=50, primaryKeyordinal:=1, _
            properties:={ObjectEntryProperty.Keyword}, validationPropertystrings:={ObjectValidationProperty.NotEmpty}, _
            XID:="WS", title:="Workspace", Description:="workspaceID identifier")> Public Const ConstFNID As String = "wspace"


        '** Fields
        <ormObjectEntry(typeid:=otDataType.Text, size:=255, isnullable:=True, _
            XID:="WS1", title:="Description")> Public Const ConstFNDescription = "desc"

        <ormObjectEntry(typeid:=otDataType.List, innertypeid:=otDataType.Text, _
            XID:="WS2", title:="forecast lookup order", description:="Forecasts milestones are lookup in this order. Must include this workspaceID ID.")> _
        Public Const ConstFNFCRelyOn = "fcrelyOn"

        <ormObjectEntry(typeid:=otDataType.List, innertypeid:=otDataType.Text, _
            XID:="WS3", title:="actual lookup order", description:="Actual milestones are looked up in this order. Must include this workspaceID ID")> _
        Public Const ConstFNActRelyOn = "actrelyOn"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
            XID:="WS4", title:="Base", description:="if set this workspaceID is a base workspaceID")> Public Const ConstFNIsBase = "isbase"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=True, dbdefaultvalue:="1", _
              XID:="WS5", title:="has actuals", description:="if set this workspaceID has actual milestones") _
               > Public Const ConstFNHasAct = "hasact"

        <ormObjectEntry(typeid:=otDataType.List, innertypeid:=otDataType.Text, isnullable:=True,
          XID:="WS6", title:="accesslist", description:="Accesslist")> Public Const ConstFNAccesslist = "acclist"

        <ormObjectEntry(typeid:=otDataType.[Long], defaultValue:=0, isnullable:=True, _
              XID:="WS7", title:="min schedule updc", description:="Minimum update counter for schedules of this workspaceID") _
               > Public Const ConstFNMinScheduleUPC = "minsupdc"

        <ormObjectEntry(typeid:=otDataType.[Long], defaultValue:=9999, isnullable:=True, _
              XID:="WS8", title:="max schedule updc", description:="Maximum update counter for schedules of this workspaceID") _
               > Public Const ConstFNMaxScheduleUPC = "maxsupdc"

        <ormObjectEntry(typeid:=otDataType.[Long], defaultValue:=0, isnullable:=True, _
              XID:="WS9", title:="min target updc", description:="Minimum update counter for targets of this workspaceID") _
               > Public Const ConstFNMinTargetUPDC = "mintupdc"

        <ormObjectEntry(typeid:=otDataType.[Long], defaultValue:=9999, isnullable:=True, _
              XID:="WS10", title:="max target updc", description:="Minimum update counter for target of this workspaceID") _
               > Public Const ConstFNMaxTargetUPDC = "maxtupdc"

        ''' <summary>
        '''  deactivate foreign keys to domain since this would end up in a mess - all the schedule and deliverable object would be
        ''' in domains then also
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID,
            useforeignkey:=otForeignKeyImplementation.None)> Public Shadows Const ConstFNDomainID = Domain.ConstFNDomainID


        ' fields
        <ormEntryMapping(EntryName:=ConstFNID)> Private _ID As String = ""
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNIsBase)> Private _isBasespace As Boolean
        <ormEntryMapping(EntryName:=ConstFNHasAct)> Private _hasActuals As Boolean
        <ormEntryMapping(EntryName:=ConstFNFCRelyOn)> Private _fcrelyingOn As String()
        <ormEntryMapping(EntryName:=ConstFNActRelyOn)> Private _actrelyingOn As String()
        <ormEntryMapping(EntryName:=ConstFNAccesslist)> Private _accesslistID As String()

        <ormEntryMapping(EntryName:=ConstFNMinScheduleUPC)> Private _min_schedule_updc As Long
        <ormEntryMapping(EntryName:=ConstFNMaxScheduleUPC)> Private _max_schedule_updc As Long
        <ormEntryMapping(EntryName:=ConstFNMinTargetUPDC)> Private _min_target_updc As Long
        <ormEntryMapping(EntryName:=ConstFNMaxTargetUPDC)> Private _max_target_updc As Long

        ' dynamics
        Private _fc_wspace_stack As New List(Of String)
        Private _act_wspace_stack As New List(Of String)

        ' further internals

        ''' <summary>
        ''' constructor of a clsOTDBDefWorkspace
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub
#Region "Properties"
        ''' <summary>
        ''' Gets or sets the domain ID.
        ''' </summary>
        ''' <value>The domain ID.</value>
        Public Property DomainID() As String
            Get
                Return Me._domainID
            End Get
            Set(value As String)
                SetValue(ConstFNDomainID, value)
            End Set
        End Property

        ''' <summary>
        ''' get the ID of the Workspace
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        <ormPropertyMappingAttribute(ID:="ID", fieldname:=ConstFNID, tablename:=ConstTableID)> ReadOnly Property ID() As String
            Get
                Return _ID
            End Get

        End Property

        Public Property Description() As String
            Get
                Return _description
            End Get
            Set(value As String)
                SetValue(ConstFNDescription, value)
            End Set
        End Property


        Public Property IsBasespace() As Boolean
            Get
                IsBasespace = _isBasespace
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsBase, value)
            End Set
        End Property

        Public Property HasActuals() As Boolean
            Get
                Return _hasActuals
            End Get
            Set(value As Boolean)
                SetValue(ConstFNHasAct, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the forecast milestone workspaces in order 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Property FCRelyingOn() As String()
            Get
                Return _fcrelyingOn
            End Get
            Set(value As String())
                SetValue(ConstFNFCRelyOn, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the actuals milestone workspace order
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Property ACTRelyingOn() As String()
            Get

                Return _actrelyingOn
            End Get
            Set(value As String())
                SetValue(ConstFNActRelyOn, value)

            End Set
        End Property

        ''' <summary>
        ''' gets or set the access list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property AccesslistIDs() As String()
            Get
                Return _accesslistID
            End Get
            Set(value As String())
                SetValue(ConstFNAccesslist, value)
            End Set
        End Property
        ''' <summary>
        ''' get or set the minimum schedule updc
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MinScheduleUPDC() As Long
            Get
                Return _min_schedule_updc
            End Get
            Set(value As Long)
                SetValue(ConstFNMinScheduleUPC, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the maximum schedule updc
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MaxScheduleUPDC() As Long
            Get
                Return _max_schedule_updc
            End Get
            Set(value As Long)
                SetValue(ConstFNMaxScheduleUPC, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the minimum target updc
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MinTargetUPDC() As Long
            Get
                Return _min_target_updc
            End Get
            Set(value As Long)
                SetValue(ConstFNMinTargetUPDC, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the maximum target updc
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MaxTargetUPDC() As Long
            Get
                Return _max_target_updc
            End Get
            Set(value As Long)
                SetValue(ConstFNMaxTargetUPDC, value)
            End Set
        End Property

#End Region


        ''' <summary>
        ''' Retrieve the workspaceID Cache Object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal id As String, Optional domainid As String = "", Optional forcereload As Boolean = False) As Workspace
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim pkarray() As Object = {UCase(id)}
            Return Retrieve(Of Workspace)(pkArray:=pkarray, domainID:=domainid, forceReload:=forcereload)
        End Function


        ''' <summary>
        ''' creates with this object a new persistable Def workspaceID
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal workspaceID As String, Optional ByVal domainid As String = "") As Workspace
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim primarykey() As Object = {workspaceID.ToUpper}
            Return ormDataObject.CreateDataObject(Of Workspace)(pkArray:=primarykey, domainID:=domainid, checkUnique:=True)
        End Function


        ''' <summary>
        ''' returns a List(of clsotdbDefWorkspace) of all workspaceID Definitions
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of Workspace)
            Return ormDataObject.AllDataObject(Of Workspace)()
        End Function

    End Class

    ''' <summary>
    ''' Domain Definition Class
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(version:=1, id:=Domain.ConstObjectID, description:="domain definition for horizontal grouping of objects", _
        modulename:=ConstModuleCore, isbootstrap:=True, useCache:=True)> Public Class Domain
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable


        '** const
        Public Const ConstObjectID = "Domain"
        <ormSchemaTableAttribute(Version:=1, usecache:=True)> Public Const ConstTableID As String = "tblDefDomains"

        '** key
        <ormObjectEntry(XID:="DM1", _
            typeid:=otDataType.Text, size:=50, Properties:={ObjectEntryProperty.Keyword}, _
            validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
            title:="Domain", Description:="domain identifier", _
            primaryKeyordinal:=1, isnullable:=False, useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID As String = "DOMAINID"

        '** fields
        <ormObjectEntry(XID:="DM2", _
            typeid:=otDataType.Text, size:=100, _
            title:="Description", Description:="description of the domain")> Public Const ConstFNDescription = "desc"

        <ormObjectEntry(XID:="DM3", _
            typeid:=otDataType.Bool, title:="Global", description:="if set this domain is the global domain") _
             > Public Const ConstFNIsGlobal = "isglobal"

        <ormObjectEntry(XID:="DM10", _
              typeid:=otDataType.[Long], defaultValue:=0, dbdefaultvalue:="0", _
              title:="min deliverable uid", description:="Minimum deliverable uid for domain")> Public Const ConstFNMinDeliverableUID = "mindlvuid"

        <ormObjectEntry(XID:="DM11", _
              typeid:=otDataType.[Long], defaultValue:=999999, dbdefaultvalue:="999999", _
              title:="max deliverable uid", description:="Maximum deliverable uid for domain")> Public Const ConstFNMaxDeliverableUID = "maxdlvuid"


        ' field mappings
        <ormEntryMapping(EntryName:=ConstFNDomainID)> Private _ID As String = ""
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNIsGlobal)> Private _isGlobal As Boolean

        <ormEntryMapping(EntryName:=ConstFNMinDeliverableUID)> Private _min_deliverable_uid As Long
        <ormEntryMapping(EntryName:=ConstFNMaxDeliverableUID)> Private _max_deliverable_uid As Long

        ' dynamics
        Private _settings As New Dictionary(Of String, DomainSetting)
        Public Event OnInitialize As EventHandler(Of DomainEventArgs)
        Public Event OnReset As EventHandler(Of DomainEventArgs)

        Private _SessionDir As New Dictionary(Of String, Session)

        ''' <summary>
        ''' constructor of a clsOTDBDefWorkspace
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub

#Region "Properties"
        ''' <summary>
        ''' returns the ID of this domain
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ormPropertyMappingAttribute(ID:="ID", fieldname:=ConstFNDomainID, tablename:=ConstTableID)> ReadOnly Property ID() As String
            Get
                ID = _ID
            End Get

        End Property
        ''' <summary>
        ''' gets and sets the description text of the domain
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
            End Set
        End Property
        ''' <summary>
        ''' gets and set the Global Flag of the domain
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsGlobal() As Boolean
            Get
                IsGlobal = _isGlobal
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNIsGlobal, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets or set the minimum deliverable UID for this domain
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MinDeliverableUID() As Long
            Get
                MinDeliverableUID = _min_deliverable_uid
            End Get
            Set(value As Long)
                SetValue(entryname:=ConstFNMinDeliverableUID, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets or set the maximum Deliverable UID for this domain
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MaxDeliverableUID() As Long
            Get
                MaxDeliverableUID = _max_deliverable_uid
            End Get
            Set(value As Long)
                SetValue(entryname:=ConstFNMaxDeliverableUID, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets a list of domain settings
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Settings() As List(Of DomainSetting)
            Get
                Return _settings.Values.ToList
            End Get
        End Property
#End Region

        ''' <summary>
        ''' returns a SQL String to insert the Gloobal Domain in the table -> bootstrap
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetInsertGlobalDomainSQLString(domainid As String, description As String, mindeliverableuid As Long, maxdeliverableuid As Long) As String

            Dim aSqlString As String = String.Format("INSERT INTO {0} ", ConstTableID)
            aSqlString &= String.Format("( [{0}], [{1}], [{2}], [{3}],  [{4}], [{5}], [{6}])", _
                                         ConstFNDomainID, ConstFNDescription, ConstFNIsGlobal, ConstFNMinDeliverableUID, ConstFNMaxDeliverableUID, _
                                         ConstFNCreatedOn, ConstFNUpdatedOn)
            If CurrentDBDriver.Type = otDbDriverType.ADONETSQL Then
                aSqlString &= String.Format("VALUES ('{0}','{1}', {2}, {3}, {4},'{5}', '{6}' )", _
                                            domainid, description, 1, mindeliverableuid, maxdeliverableuid, _
                                             Date.Now.ToString("yyyy-MM-ddThh:mm:ss"), Date.Now.ToString("yyyy-MM-ddThh:mm:ss"))
            ElseIf CurrentDBDriver.Type = otDbDriverType.ADONETOLEDB Then
                aSqlString &= String.Format("VALUES ('{0}','{1}', {2}, {3}, {4},'{5}', '{6}' )", _
                                           domainid, description, 1, mindeliverableuid, maxdeliverableuid, _
                                            Date.Now.ToString("yyyy-MM-ddThh:mm:ss"), Date.Now.ToString("yyyy-MM-ddThh:mm:ss"))
            Else
                CoreMessageHandler(message:="database type must be implemented in routine sql", messagetype:=otCoreMessageType.InternalError, _
                                   subname:="Domain.GetInsertGlobaldomainSQLString")
            End If


            Return aSqlString

        End Function
        ''' <summary>
        ''' handles the session start event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnSessionEnd(sender As Object, e As SessionEventArgs)
            If _SessionDir.ContainsKey(e.Session.SessionID) Then
                _SessionDir.Remove(e.Session.SessionID)
            End If

        End Sub
        ''' <summary>
        ''' handles the session end event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>

        Public Sub OnSessionStart(sender As Object, e As SessionEventArgs)

        End Sub
        ''' <summary>
        ''' Register a Session a the Domain
        ''' </summary>
        ''' <param name="session"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RegisterSession(session As Session) As Boolean
            If _SessionDir.ContainsKey(session.SessionID) Then
                _SessionDir.Remove(session.SessionID)
            End If
            _SessionDir.Add(session.SessionID, session)
            AddHandler session.OnStarted, AddressOf OnSessionStart
            AddHandler session.OnEnding, AddressOf OnSessionEnd

        End Function


        Public Shared Function GlobalDomain() As Domain
            Return Retrieve(id:=ConstGlobalDomain)
        End Function
        ''' <summary>
        ''' Retrieve the workspaceID Cache Object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal id As String, Optional dbdriver As iormDatabaseDriver = Nothing, Optional runtimeOnly As Boolean = False, Optional forcereload As Boolean = False) As Domain
            Dim pkarray() As Object = {UCase(id)}
            Return Retrieve(Of Domain)(pkArray:=pkarray, dbdriver:=dbdriver, runtimeOnly:=runtimeOnly, forceReload:=forcereload)
        End Function

        ''' <summary>
        ''' returns true if the setting exists
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasSetting(id As String) As Boolean
            Return _settings.ContainsKey(key:=id.ToUpper)
        End Function
        ''' <summary>
        ''' returns the setting valid in the domain
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSetting(id As String) As DomainSetting
            If Me.HasSetting(id:=id) Then
                Return _settings.Item(key:=id.ToUpper)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' sets (add or overwrites) the setting valid in the domain
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetSetting(id As String, datatype As otDataType, value As Object, Optional description As String = "") As Boolean
            Dim aSetting As New DomainSetting
            If Me.HasSetting(id:=id) Then
                aSetting = Me.GetSetting(id:=id)
            Else
                aSetting = DomainSetting.Create(domainID:=Me.ID, id:=id)
                If aSetting Is Nothing Then aSetting = DomainSetting.Retrieve(domainID:=Me.ID, id:=id)
            End If

            If aSetting Is Nothing OrElse Not aSetting.IsAlive(throwError:=False) Then
                Return False
            End If
            aSetting.Datatype = datatype
            aSetting.value = value
            aSetting.Description = description

            If Not Me.HasSetting(id:=id) Then _settings.Add(key:=id.ToUpper, value:=aSetting)
            Return True
        End Function
        ''' <summary>
        ''' Load the settings to the settings dictionary
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadSettings() As Boolean
            Dim aListDomain As New List(Of DomainSetting)
            If ConstGlobalDomain <> Me.ID Then aListDomain = DomainSetting.RetrieveByDomain(domainID:=Me.ID)
            Dim aListGlobal As List(Of DomainSetting) = DomainSetting.RetrieveByDomain(domainID:=ConstGlobalDomain)

            '** first for the global
            For Each aSetting In aListGlobal
                If _settings.ContainsKey(key:=aSetting.ID) Then
                    _settings.Remove(key:=aSetting.ID)
                End If
                _settings.Add(key:=aSetting.ID, value:=aSetting)
            Next

            '** overwrite
            For Each aSetting In aListDomain
                If _settings.ContainsKey(key:=aSetting.ID) Then
                    _settings.Remove(key:=aSetting.ID)
                End If
                _settings.Add(key:=aSetting.ID, value:=aSetting)
            Next
            Return True
        End Function
        ''' <summary>
        ''' Persist the data object
        ''' </summary>
        ''' <param name="TIMESTAMP"></param>
        ''' <param name="ForceSerializeToOTDB"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub OnPersist(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnPersisted
            Try
                Dim myself = TryCast(e.DataObject, Domain)
                For Each aSetting In myself.Settings
                    aSetting.Persist()
                Next

            Catch ex As Exception
                Call CoreMessageHandler(subname:="Domain.OnPersisted", exception:=ex)
            End Try
        End Sub

        ''' <summary>
        ''' infuse the domain  by a record and load the settings
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub OnInfused(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnInfused

            Try

                If Not LoadSettings() Then
                End If

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="Domain.Infuse")
            End Try
        End Sub
        ''' <summary>
        ''' create the objects persistence schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of Domain)(silent:=silent)
        End Function

        ''' <summary>
        ''' creates with this object a new persistable Def workspaceID
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal ID As String, Optional runtimeonly As Boolean = False) As Domain
            Dim primarykey() As Object = {ID.ToUpper}
            Return ormDataObject.CreateDataObject(Of Domain)(pkArray:=primarykey, runtimeOnly:=runtimeonly, checkUnique:=Not runtimeonly)
        End Function

#Region "static routines"
        ''' <summary>
        ''' returns a List(of Domain) of all workspaceID Definitions
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of Domain)
            Return ormDataObject.AllDataObject(Of Domain)()
        End Function
#End Region
    End Class
    '************************************************************************************
    '***** CLASS clsOTDBDefOrgUnit describes additional database schema information
    '*****
    ''' <summary>
    ''' Organization Unit Definition Class
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=OrgUnit.ConstObjectID, modulename:=ConstModuleCore, description:="recursive organization unit for a group of persons", _
        Version:=1, useCache:=True, adddeletefieldbehavior:=True, addDomainBehavior:=True)> Public Class OrgUnit
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable

        '**
        Public Const ConstObjectID = "OrgUnit"
        '** Table
        <ormSchemaTable(version:=2, usecache:=True)> Public Const ConstTableID As String = "tblDefOrgUnits"

        '** primary Keys
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, primaryKeyOrdinal:=1, _
            properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty},
            XID:="OU1", title:="OrgUnit", description:="ID of the organization unit")> Public Const ConstFNID = "id"
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=2 _
         , useforeignkey:=otForeignKeyImplementation.NativeDatabase, defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '** fields
        <ormObjectEntry(typeid:=otDataType.Text, size:=255, _
           XID:="OU2", title:="Description", description:="description of the organization unit")> Public Const ConstFNDescription = "desc"
        <ormObjectEntry(referenceObjectEntry:=Person.ConstObjectID & "." & Person.constFNID, _
           XID:="OU3", title:="Manager", description:="manager of the organization unit")> Public Const ConstFNManager = "manager"
        <ormObjectEntry(referenceObjectEntry:=Site.ConstObjectiD & "." & Site.constFNId, _
          XID:="OU4", title:="Site", description:="ID of the site organization unit")> Public Const ConstFNSite = "site"
        <ormObjectEntry(referenceObjectEntry:=ConstObjectID & "." & ConstFNID, _
          XID:="OU5", title:="Superior", description:="superior ID of the  organization unit")> Public Const ConstFNSuperior = "superior"
        <ormObjectEntry(typeid:=otDataType.Text, size:=50, _
         XID:="OU6", title:="Function", description:="default function ID of the  organization unit")> Public Const ConstFNFunction = "funct"

        ' field mapping
        <ormEntryMapping(EntryName:=ConstFNID)> Private _id As String = ""
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNManager)> Private _manager As String = ""
        <ormEntryMapping(EntryName:=ConstFNSite)> Private _siteid As String = ""
        <ormEntryMapping(EntryName:=ConstFNSuperior)> Private _superiorOUID As String = ""
        <ormEntryMapping(EntryName:=ConstFNFunction)> Private _functionid As String = ""

        ''' <summary>
        ''' constructor of a DefOrgUnit
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New(ConstTableID)
        End Sub

#Region "Properties"
        ReadOnly Property ID() As String
            Get
                ID = _id
            End Get

        End Property

        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                _description = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Manager() As String
            Get
                Manager = _manager
            End Get
            Set(value As String)
                _manager = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Siteid() As String
            Get
                Siteid = _siteid
            End Get
            Set(value As String)
                _siteid = value
                Me.IsChanged = True
            End Set
        End Property


        Public Property SuperiorOUID() As String
            Get
                SuperiorOUID = _superiorOUID
            End Get
            Set(value As String)
                _superiorOUID = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Functionid() As String
            Get
                Functionid = _functionid
            End Get
            Set(value As String)
                _functionid = value
                Me.IsChanged = True
            End Set
        End Property
#End Region


        ''' <summary>
        ''' Retrieve 
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal id As String, Optional domainID As String = "", Optional forcereload As Boolean = False) As OrgUnit
            Return Retrieve(Of OrgUnit)(pkArray:={domainID, id}, domainID:=domainID, forceReload:=forcereload)
        End Function


        ''' <summary>
        ''' returns a collection of all objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function All() As List(Of OrgUnit)
            Return ormDataObject.AllDataObject(Of OrgUnit)()
        End Function
        '**** create : create a new Object with primary keys
        '****
        Public Shared Function Create(ByVal id As String, Optional domainID As String = "") As OrgUnit
            Dim primarykey() As Object = {id, domainID}
            ' set the primaryKey
            Return CreateDataObject(Of OrgUnit)(primarykey, domainID:=domainID, checkUnique:=True)
        End Function

    End Class


    ''' <summary>
    ''' Site Definition Class
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=Site.ConstObjectiD, description:="site (geographic units) definition for organization units", modulename:=ConstModuleCore, _
        Version:=1, useCache:=True, addDomainBehavior:=True, adddeletefieldbehavior:=True)> Public Class Site
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        '** ObjectID
        Public Const ConstObjectiD = "Site"
        '** Table
        <ormSchemaTable(version:=2, useCache:=True)> Public Const ConstTableID As String = "tblDefOUSites"

        '** keys
        <ormObjectEntry(typeid:=otDataType.Text, size:=50, primarykeyordinal:=1, _
            XID:="OUS1", title:="Site ID", description:="id of the site")> Public Const constFNId = "id"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=2 _
         , useforeignkey:=otForeignKeyImplementation.NativeDatabase, defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID


        '** fields
        <ormObjectEntry(referenceObjecTEntry:=CalendarEntry.ConstObjectID & "." & CalendarEntry.constFNName, _
            XID:="OUS2", title:="CalendarName", description:="name of the calendar valid for this site")> Public Const ConstFNCalendarID = "calendar"

        <ormObjectEntry(typeid:=otDataType.Memo, XID:="OUS10", title:="Description", description:="description of the site")> Public Const constFNDescription = "desc"
        ' field mapping
        <ormEntryMapping(EntryName:=constFNId)> Private _iD As String = ""
        <ormEntryMapping(EntryName:=constFNId)> Private _CalendarID As String = ""
        <ormEntryMapping(EntryName:=constFNDescription)> Private _description As String = ""
        ''' <summary>
        ''' constructor of Def OUSite
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New(ConstTableID)

        End Sub

#Region "Properties"
        ''' <summary>
        ''' ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ID() As String
            Get
                ID = _iD
            End Get

        End Property
        ''' <summary>
        ''' Description
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                _description = value
                Me.IsChanged = True
            End Set
        End Property
#End Region

        ''' <summary>
        ''' Retrieve
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal id As String, Optional domainid As String = "", Optional forcereload As Boolean = False) As Site
            Return Retrieve(Of Site)(pkArray:={UCase(id), domainid}, domainID:=domainid, forceReload:=forcereload)
        End Function
        ''' <summary>
        ''' Load and infuse the object 
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Inject(ByVal id As String, Optional domainID As String = "") As Boolean
            Dim pkarry() As Object = {UCase(id), domainID}
            Return MyBase.Inject(pkArray:=pkarry, domainID:=domainID)
        End Function
        ''' <summary>
        ''' create the persistency object
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of Site)(silent:=silent)
        End Function
        ''' <summary>
        ''' returns a collection of all objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function All(Optional domainID As String = "") As List(Of Site)
            Return ormDataObject.AllDataObject(Of Site)(domainID:=domainID)
        End Function
        '**** create : create a new Object with primary keys
        ''' <summary>
        ''' creates a persistable site object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="domainID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal id As String, Optional domainID As String = "") As Site
            Dim primarykey() As Object = {id, domainID}
            ' set the primaryKey
            Return ormDataObject.CreateDataObject(Of Site)(primarykey, domainID:=domainID, checkUnique:=True)
        End Function

    End Class


End Namespace