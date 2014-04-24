
REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** XChangeManager Business Object Classes for On Track Database Backend Library
REM ***********
REM *********** Version: X.YY
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************
Option Explicit On
Imports System.Collections.Specialized

Imports OnTrack.Database
Imports OnTrack.Deliverables
Imports OnTrack.Commons



Namespace OnTrack.XChange

    ''' <summary>
    ''' XChangeable Interface for exchangeable objects
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface iotXChangeable
        ''' <summary>
        ''' runs the XChange 
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns>True if successful</returns>
        ''' <remarks></remarks>
        Function RunXChange(ByRef envelope As XEnvelope, Optional ByRef msglog As ObjectLog = Nothing) As Boolean

        ''' <summary>
        ''' runs the Precheck
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns>True if successful</returns>
        ''' <remarks></remarks>
        Function RunXPreCheck(ByRef envelope As XEnvelope, Optional ByRef msglog As ObjectLog = Nothing) As Boolean

    End Interface
    ''' <summary>
    ''' XChange Commands
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otXChangeCommandType
        Update = 1
        Delete = 2
        UpdateCreate = 3
        Duplicate = 4
        Read = 5
    End Enum
    ''' <summary>
    ''' otXChangeConfigEntryType
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otXChangeConfigEntryType
        [Object] = 1
        ObjectEntry
    End Enum

    ''' <summary>
    ''' Interface for XConfigMembers
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IXChangeConfigEntry
        Inherits iormPersistable
        Inherits iormInfusable

        ''' <summary>
        ''' returns the Object entryname
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Property ObjectEntryname() As String

        ''' <summary>
        ''' returns the ID of the ConfigMember
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property XID() As String

        ''' <summary>
        ''' returns the name of the Object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Objectname() As String

        ''' <summary>
        ''' returns a List of Aliases
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Aliases() As List(Of String)

        ''' <summary>
        ''' returns the configname of this Member
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Configname() As String

        ''' <summary>
        ''' Has Alias
        ''' </summary>
        ''' <param name="alias"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasAlias([alias] As String) As Boolean
        '''' <summary>
        '''' Gets the S is compund entry.
        '''' </summary>
        '''' <value>The S is compund entry.</value>
        'ReadOnly Property IsCompundEntry As Boolean

        ''' <summary>
        ''' gets the MSGLog Tag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Msglogtag() As String

        ''' <summary>
        ''' gets or sets the Xchange Command
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property XChangeCmd() As otXChangeCommandType



        ''' <summary>
        ''' Primary Key Indexno
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Indexno() As Long

        ''' <summary>
        ''' gets or sets the Xhanged Flag - value is not xchangend to and from Host Application
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property IsXChanged() As Boolean

        ''' <summary>
        ''' sets the Readonly Flag - value of the OTDB cannot be overwritten
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property IsReadOnly() As Boolean

        ''' <summary>
        ''' gets or sets the Attribute Entry Flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsObjectEntry() As Boolean

        ''' <summary>
        ''' gets True if this is a Compound
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsCompound() As Boolean

        ''' <summary>
        ''' gets True if the Attribute is a Field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsColumn() As Boolean

        ''' <summary>
        ''' gets True if this is an Object Entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsObject() As Boolean

        ''' <summary>
        ''' gets or sets the ordinal
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Ordinal() As Ordinal

        ''' <summary>
        ''' gets or sets the OrderedBy Flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property IsOrderedBy() As Boolean

        ''' <summary>
        ''' returns the type of the entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Type As otXChangeConfigEntryType
    End Interface

    ''' <summary>
    ''' describes an XChange XConfigMember Object
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <ormObject(id:=XChangeObject.ConstObjectID, usecache:=True, adddeletefieldbehavior:=True, adddomainbehavior:=True, _
        Modulename:=ConstModuleXChange, Description:="object definition for X Change configuration entry")> _
    Public Class XChangeObject
        Inherits XChangeConfigAbstractEntry
        Implements IXChangeConfigEntry


        Public Const ConstObjectID As String = "XChangeConfigObject"
#Region "Properties"

        ''' <summary>
        ''' gets or sets the Attribute Entry Flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property IsObjectEntry() As Boolean Implements IXChangeConfigEntry.IsObjectEntry
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' gets True if the Attribute is a Field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property IsColumn() As Boolean Implements IXChangeConfigEntry.IsColumn
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' gets True if this is an Object Entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property IsObject() As Boolean Implements IXChangeConfigEntry.IsObject
            Get
                Return True
            End Get
        End Property

#End Region

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase._type = otXChangeConfigEntryType.Object
        End Sub
        ''' <summary>
        ''' creates a persistable XChange member with primary Key
        ''' </summary>
        ''' <param name="configname"></param>
        ''' <param name="indexno"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal configname As String, indexno As Long, _
                                                Optional objectname As String = "", _
                                                Optional xcmd As otXChangeCommandType = otXChangeCommandType.Read,
                                                Optional domainid As String = "", _
                                                Optional runtimeonly As Boolean = False) As XChangeObject
            Dim aRecord As New ormRecord
            With aRecord
                .SetValue(ConstFNXConfigID, configname.ToUpper)
                .SetValue(constFNIDNo, indexno)
                .SetValue(ConstFNObjectID, objectname.ToUpper)
                .SetValue(ConstFNDomainID, domainid)
                .SetValue(constFNXCMD, xcmd)
                .SetValue(ConstFNTypeid, otXChangeConfigEntryType.Object)
                .SetValue(constFNOrderNo, indexno)
                .SetValue(constFNordinal, indexno)
            End With
            Return ormDataObject.CreateDataObject(Of XChangeObject)(aRecord, domainID:=domainid, checkUnique:=True, runtimeOnly:=runtimeonly)
        End Function

        ''' <summary>
        ''' retrieves a persistable XChange Object
        ''' </summary>
        ''' <param name="configname"></param>
        ''' <param name="indexno"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal configname As String, indexno As Long, Optional domainid As String = "", Optional runtimeonly As Boolean = False) As XChangeObject
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.Retrieve(Of XChangeObject)({configname.ToUpper, indexno, domainid}, runtimeOnly:=runtimeonly)
        End Function
    End Class
    ''' <summary>
    ''' describes object entry definition for X Change configuration entry
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=XChangeObjectEntry.ConstObjectID, usecache:=True, adddeletefieldbehavior:=True, adddomainbehavior:=True, _
        Modulename:=ConstModuleXChange, Description:="object entry definition for X Change configuration entry")> _
    Public Class XChangeObjectEntry
        Inherits XChangeConfigAbstractEntry
        Implements IXChangeConfigEntry

        Public Const ConstObjectID As String = "XChangeConfigObjectEntry"

#Region "Properties"


        ''' <summary>
        ''' gets or sets the Attribute Entry Flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property IsObjectEntry() As Boolean Implements IXChangeConfigEntry.IsObjectEntry
            Get
                Return True
            End Get

        End Property

        ''' <summary>
        ''' gets True if this is an Object Entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property IsObject() As Boolean Implements IXChangeConfigEntry.IsObject
            Get
                Return False
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the Dynamic Flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsDynamicAttribute() As Boolean
            Get
                Return _isDynamicAttribute
            End Get
            Set(value As Boolean)
                SetValue(constFNIsDynamic, value)
            End Set
        End Property
#End Region

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase._type = otXChangeConfigEntryType.ObjectEntry
        End Sub

        ''' <summary>
        ''' creates a persistable XChange Objectentry
        ''' </summary>
        ''' <param name="configname"></param>
        ''' <param name="indexno"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal configname As String, indexno As Long, Optional domainid As String = "", Optional runtimeonly As Boolean = False) As XChangeObjectEntry
            Dim aRecord As New ormRecord
            With aRecord
                .SetValue(ConstFNXConfigID, configname.ToUpper)
                .SetValue(constFNIDNo, indexno)
                .SetValue(ConstFNDomainID, domainid)
                .SetValue(ConstFNTypeid, otXChangeConfigEntryType.ObjectEntry)
            End With
            Return ormDataObject.CreateDataObject(Of XChangeObjectEntry)(aRecord, domainID:=domainid, checkUnique:=True, runtimeOnly:=runtimeonly)
        End Function

        ''' <summary>
        ''' retrieves a persistable XChange Object Entry
        ''' </summary>
        ''' <param name="configname"></param>
        ''' <param name="indexno"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal configname As String, indexno As Long, Optional domainid As String = "", Optional runtimeonly As Boolean = False) As XChangeObjectEntry
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.Retrieve(Of XChangeObjectEntry)({configname.ToUpper, indexno, domainid}, domainID:=domainid, runtimeOnly:=runtimeonly)
        End Function
    End Class

    ''' <summary>
    ''' abstract class to describe an XChangeConfiguration EntryMember - an individual item
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=XChangeConfigAbstractEntry.ConstObjectID, usecache:=True, adddeletefieldbehavior:=True, adddomainbehavior:=True, _
        Modulename:=ConstModuleXChange, Description:="abstract entry definition for X Change configuration")> _
    Public MustInherit Class XChangeConfigAbstractEntry
        Inherits ormDataObject
        Implements iormInfusable, iormPersistable, IXChangeConfigEntry

        Public Const ConstObjectID = "XChangeConfigAbstractEntry"
        ''' <summary>
        ''' Table
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTableAttribute(version:=3, usecache:=True)> Public Const ConstTableID = "tblXChangeConfigEntries"

        ''' <summary>
        ''' keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceObjectEntry:=XChangeConfiguration.constObjectID & "." & XChangeConfiguration.constFNID, primaryKeyordinal:=1, _
                        title:="XChangeConfigID", description:="name of the eXchange Configuration")> Public Const ConstFNXConfigID = XChangeConfiguration.constFNID

        <ormObjectEntry(typeid:=otDataType.Long, primaryKeyordinal:=2,
                        title:="IndexNo", description:="unique id in the the eXchange Configuration")> Public Const constFNIDNo = "IDNO"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=3, _
           useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        ''' <summary>
        ''' foreign key
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaForeignKey(useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            entrynames:={ConstFNXConfigID, ConstFNDomainID}, _
            foreignkeyreferences:={XChangeConfiguration.constObjectID & "." & XChangeConfiguration.constFNID, _
            XChangeConfiguration.constObjectID & "." & XChangeConfiguration.ConstFNDomainID})> Public Const constFKXConfig = "FK_XCONFIG"

        ''' <summary>
        ''' fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceObjectEntry:=ObjectDefinition.ConstObjectID & "." & ObjectDefinition.ConstFNID, _
                        useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNObjectID = "objectID"

        <ormObjectEntry(referenceObjectEntry:=ObjectColumnEntry.ConstObjectID & "." & ObjectColumnEntry.ConstFNEntryName, _
                       isnullable:=True)> Public Const ConstFNEntryname = "entryname" ' might be null since only object are also members

        <ormObjectEntry(typeid:=otDataType.Text, isnullable:=True,
                        title:="Description", description:="Description of the member")> Public Const ConstFNDesc = "desc"

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, isnullable:=True,
                        properties:={ObjectEntryProperty.Keyword}, _
                        title:="XChange ID", description:="ID  of the Attribute in theObjectDefinition")> Public Const ConstFNXID = "id"

        <ormObjectEntry(typeid:=otDataType.Text, isnullable:=True,
                        title:="ordinal", description:="ordinal for the Attribute Mapping")> Public Const constFNordinal = "ORDINALVALUE"

        <ormObjectEntry(typeid:=otDataType.Text, title:="Type", defaultvalue:=otXChangeConfigEntryType.ObjectEntry, isnullable:=True, _
            description:="type of the XChange configuration entry")> Public Const ConstFNTypeid = "typeid"


        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
            title:="Is Entry Read-Only", description:="Set if this entry is read-only - value in OTDB cannot be overwritten")>
        Public Const constFNIsReadonly = "isreadonly"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
            title:="Is ordered", description:="Set if this entry is ordered")>
        Public Const constFNIsOrder = "isorder"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
            title:="Is dynamic attribute", description:="Set if this entry is dynamic")>
        Public Const constFNIsDynamic = "isdynamic"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
            title:="Attribute is not exchanged", description:="Set if this attribute is not exchanged")>
        Public Const constFNIsNotXChanged = "isnotxchg"

        <ormObjectEntry(typeid:=otDataType.Text, isnullable:=True,
                        properties:={ObjectEntryProperty.Keyword}, _
                        title:="XChange Command", description:="XChangeCommand to run on this")> Public Const constFNXCMD = "xcmd"

        <ormObjectEntry(typeid:=otDataType.Long, isnullable:=True,
            title:="Order Number", description:="ordinal number in which entriy is processed")>
        Public Const constFNOrderNo = "orderno"

        <ormObjectEntry(typeid:=otDataType.Text, size:=250, isnullable:=True, _
            title:="MessageLogTag", description:="Message Log Tag")>
        Public Const constFNMsgLogTag = "msglogtag"

        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>
        <ormEntryMapping(EntryName:=ConstFNXConfigID)> Protected _configname As String = ""
        <ormEntryMapping(EntryName:=constFNIDNo)> Protected _idno As Long

        <ormEntryMapping(EntryName:=ConstFNXID)> Protected _xid As String

        <ormEntryMapping(EntryName:=ConstFNObjectID)> Protected _objectname As String
        <ormEntryMapping(EntryName:=ConstFNEntryname)> Protected _entryname As String

        '<otColumnMapping(ColumnName:=ConstFNordinal)> do not since we cannot map it
        Private _ordinal As Ordinal = New Ordinal(0)

        <ormEntryMapping(EntryName:=ConstFNDesc)> Protected _desc As String = ""
        <ormEntryMapping(EntryName:=constFNIsNotXChanged)> Protected _isNotXChanged As Boolean
        <ormEntryMapping(EntryName:=constFNIsReadonly)> Protected _isReadOnly As Boolean

        <ormEntryMapping(EntryName:=ConstFNTypeid)> Protected _type As otXChangeConfigEntryType

        <ormEntryMapping(EntryName:=constFNXCMD)> Protected _xcmd As otXChangeCommandType = 0
        <ormEntryMapping(EntryName:=constFNIsOrder)> Protected _isOrdered As Boolean
        <ormEntryMapping(EntryName:=constFNOrderNo)> Protected _orderNo As Long
        <ormEntryMapping(EntryName:=constFNIsDynamic)> Protected _isDynamicAttribute As Boolean

        'dynamic
        Protected _EntryDefinition As iormObjectEntry
        Protected _ObjectDefinition As ObjectDefinition
        ' Protected _aliases As String()    ' not saved !
        Protected _msglog As New ObjectLog
        Protected _msglogtag As String

        '** initialize
        Public Sub New()
            Call MyBase.New(ConstTableID)

            _EntryDefinition = Nothing
        End Sub

#Region "Properties"


        ''' <summary>
        ''' gets or sets the XChange ID for the Member
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property XID() As String Implements IXChangeConfigEntry.XID
            Get
                Return _xid
            End Get
            Set(value As String)
                SetValue(ConstFNXID, value)
            End Set
        End Property

        ''' <summary>
        ''' gets the entryname of the data object data object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ObjectEntryname() As String Implements IXChangeConfigEntry.ObjectEntryname
            Get
                Return _entryname
            End Get
            Set(value As String)
                SetValue(ConstFNEntryname, value)
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the objectname to which the entry belongs
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Objectname() As String Implements IXChangeConfigEntry.Objectname
            Get
                Return _objectname
            End Get
            Set(value As String)
                SetValue(ConstFNObjectID, value)
            End Set
        End Property

        '****** getUniqueTag
        Public Function GetUniqueTag()
            GetUniqueTag = ConstDelimiter & ConstTableID & ConstDelimiter & _configname & ConstDelimiter & _xid & ConstDelimiter & _objectname & ConstDelimiter & _entryname & ConstDelimiter
        End Function
        ''' <summary>
        ''' gets the MSGLog Tag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Msglogtag() As String Implements IXChangeConfigEntry.Msglogtag
            Get
                If _msglogtag = "" Then
                    _msglogtag = GetUniqueTag()
                End If
                Msglogtag = _msglogtag
            End Get

        End Property

        ''' <summary>
        ''' gets or sets the objectname to which the entry belongs
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Type() As otXChangeConfigEntryType Implements IXChangeConfigEntry.Type
            Get
                Return _type
            End Get
            Set(value As otXChangeConfigEntryType)
                SetValue(ConstFNTypeid, value)
            End Set
        End Property
        ''' <summary>
        ''' gets the configname
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Configname() As String Implements IXChangeConfigEntry.Configname
            Get
                Return _configname
            End Get
            Set(value As String)
                SetValue(ConstFNXConfigID, value)
            End Set
        End Property


        ''' <summary>
        ''' gets the Aliases of the XChange Member
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Aliases() As List(Of String) Implements IXChangeConfigEntry.Aliases
            Get
                If Not Me.[ObjectEntryDefinition] Is Nothing Then
                    Return _EntryDefinition.Aliases.ToList
                Else
                    Return New List(Of String)
                End If
            End Get

        End Property

        ''' <summary>
        ''' gets true if the XChangeMember has the Alias
        ''' </summary>
        ''' <param name="alias"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasAlias([alias] As String) As Boolean Implements IXChangeConfigEntry.HasAlias
            Get
                If Me.[ObjectEntryDefinition] IsNot Nothing Then
                    Return Me.[ObjectEntryDefinition].Aliases.Count = 0
                Else
                    Return False
                End If
            End Get
        End Property

        ''' <summary>
        ''' gets or sets the Xchange Command
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property XChangeCmd() As otXChangeCommandType Implements IXChangeConfigEntry.XChangeCmd
            Get
                Return _xcmd
            End Get
            Set(value As otXChangeCommandType)
                SetValue(constFNXCMD, value)
            End Set
        End Property
        ''' <summary>
        ''' gets the ObjectEntry Definition for the XChange Member
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property [ObjectEntryDefinition] As iormObjectEntry
            Get
                If _EntryDefinition Is Nothing AndAlso IsAlive(throwError:=False) Then

                    If _entryname IsNot Nothing And Me.Objectname IsNot Nothing Then
                        _EntryDefinition = CurrentSession.Objects.GetEntry(objectname:=Me.Objectname, entryname:=Me.ObjectEntryname)
                    ElseIf Me.Objectname IsNot Nothing And Me.XID IsNot Nothing Then
                        _EntryDefinition = CurrentSession.Objects.GetEntryByXID(xid:=_xid, objectname:=Me.Objectname).First
                    Else
                        _EntryDefinition = CurrentSession.Objects.GetEntryByXID(xid:=_xid).First
                    End If

                End If

                Return _EntryDefinition
            End Get

        End Property
        ''' <summary>
        ''' Object Definition
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property [ObjectDefinition] As ObjectDefinition
            Get
                Dim aDefinition As ObjectDefinition

                If (Me.IsCreated Or Me.IsLoaded) And _ObjectDefinition Is Nothing Then
                    If Me.Objectname <> "" Then
                        aDefinition = CurrentSession.Objects.GetObject(Me.Objectname)
                        If Not aDefinition Is Nothing Then
                            _ObjectDefinition = aDefinition
                        End If
                        Return _ObjectDefinition
                    End If
                End If

                ' return
                Return _ObjectDefinition
            End Get
            Set(value As ObjectDefinition)
                _ObjectDefinition = value
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the ordinal
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Ordinal() As Ordinal Implements IXChangeConfigEntry.Ordinal
            Get
                Ordinal = _ordinal
            End Get
            Set(value As Ordinal)
                _ordinal = value
                Me.IsChanged = True
            End Set
        End Property


        ''' <summary>
        ''' Primary Key Indexno
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Indexno() As Long Implements IXChangeConfigEntry.Indexno
            Get
                Indexno = _idno
            End Get
            Set(value As Long)
                If _idno <> value Then
                    _idno = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the Xhanged Flag - value is not xchangend to and from Host Application
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsXChanged() As Boolean Implements IXChangeConfigEntry.IsXChanged
            Get
                Return Not _isNotXChanged
            End Get
            Set(value As Boolean)
                SetValue(constFNIsNotXChanged, Not value)
            End Set
        End Property

        ''' <summary>
        ''' sets the Readonly Flag - value of the OTDB cannot be overwritten
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsReadOnly() As Boolean Implements IXChangeConfigEntry.IsReadOnly
            Get
                Return _isReadOnly
            End Get
            Set(value As Boolean)
                SetValue(constFNIsReadonly, value)
            End Set
        End Property
        ''' <summary>
        ''' returns true if this entry is an object entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property IsObjectEntry() As Boolean Implements IXChangeConfigEntry.IsObjectEntry
            Get
                Return Me.Type = otXChangeConfigEntryType.ObjectEntry
            End Get
        End Property

        ''' <summary>
        ''' gets True if this is a Compound
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property IsCompound() As Boolean Implements IXChangeConfigEntry.IsCompound
            Get
                If Me.Type = otXChangeConfigEntryType.ObjectEntry Then
                    Dim anObjectEntry = Me.ObjectEntryDefinition
                    If anObjectEntry IsNot Nothing Then
                        Return anObjectEntry.IsCompound
                    Else
                        Return False
                    End If
                Else
                    Return False
                End If
            End Get

        End Property
        ''' <summary>
        ''' gets True if the Attribute is a Column
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property IsColumn() As Boolean Implements IXChangeConfigEntry.IsColumn
            Get
                If Me.Type = otXChangeConfigEntryType.ObjectEntry Then
                    Dim anObjectEntry = Me.ObjectEntryDefinition
                    If anObjectEntry IsNot Nothing Then
                        Return anObjectEntry.IsColumn
                    Else
                        Return False
                    End If
                Else
                    Return False
                End If
            End Get
        End Property

        ''' <summary>
        ''' gets True if this is entry is an Object 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property IsObject() As Boolean Implements IXChangeConfigEntry.IsObject
            Get
                Return Me.Type = otXChangeConfigEntryType.Object
            End Get
        End Property

        ''' <summary>
        ''' gets or sets the OrderedBy Flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsOrderedBy() As Boolean Implements IXChangeConfigEntry.IsOrderedBy
            Get
                Return _isOrdered
            End Get
            Set(value As Boolean)
                SetValue(constFNIsOrder, value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the Order ordinal
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Orderno() As Long
            Get
                Return _orderNo
            End Get
            Set(value As Long)
                SetValue(constFNOrderNo, value)
            End Set
        End Property

#End Region

        ''' <summary>
        ''' Increment ordinal
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Incordinal() As Ordinal
            If IsNumeric(_ordinal) Then
                _ordinal = New Ordinal(_ordinal.ToUInt64 + 1)
            ElseIf IsEmpty(_ordinal) Then
                _ordinal = New Ordinal(1)
            Else
                Call CoreMessageHandler(subname:="XConfigMember.incordinal", message:="ordinal is not numeric")
                Incordinal = Nothing
                Exit Function
            End If
            Incordinal = _ordinal
        End Function

        ''' <summary>
        ''' infuses the XChange member from the record
        ''' </summary>
        ''' <param name="record"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub XChangeConfigAbstractEntry_OnInfused(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnInfused
            Dim aValue As Object

            Try
                Dim isnull As Boolean
                aValue = Record.GetValue(constFNordinal, isNull:=isnull)
                If isnull Then
                    _ordinal = New Ordinal(0)
                Else
                    If IsNumeric(aValue) Then
                        _ordinal = New Ordinal(CLng(aValue))
                    Else
                        _ordinal = New Ordinal(CStr(aValue))
                    End If
                End If
                e.Proceed = True
                Exit Sub
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="XChangeConfigAbstractEntry_OnInfused.OnInfused")
                e.AbortOperation = True
            End Try

        End Sub

        ''' <summary>
        ''' infuses the XChange member from the record
        ''' </summary>
        ''' <param name="record"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub XChangeConfigAbstractEntry_OnFed(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnFed

            Try
                e.Record.SetValue(constFNordinal, _ordinal.Value.ToString)
                If Orderno = 0 And Me.Ordinal <> New Ordinal(0) And Me.Ordinal.Type = OrdinalType.longType Then
                    Me.Orderno = Me.Ordinal.Value
                    e.Record.SetValue(constFNOrderNo, _ordinal.Value.ToString)
                End If
                e.Result = True
                e.Proceed = True
                Exit Sub
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="XchangeConfigAbstractEntry.XChangeConfigAbstractEntry_OnFed")
                e.AbortOperation = True
            End Try

        End Sub



    End Class

    ''' <summary>
    ''' CLASS XConfig defines how data can be exchanged with the XChange Manager
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(ID:=XChangeConfiguration.constObjectID, version:=1, usecache:=True, adddomainbehavior:=True, adddeletefieldbehavior:=True, _
        modulename:=ConstModuleXChange, description:="defines how data can be exchanged with the XChange Manager")> _
    Public Class XChangeConfiguration
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        'Implements iOTDBXChange
        Public Const constObjectID = "XChangeConfig"

        ''' <summary>
        ''' Table
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTableAttribute(Version:=2, usecache:=True)> Public Const constTableID = "tblXChangeConfigs"

        ''' <summary>
        ''' Keys
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        <ormObjectEntry(typeid:=otDataType.Text, size:=50, primaryKeyordinal:=1, _
             properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
             Title:="Name", Description:="Name of XChange Configuration")> Public Const constFNID = "configname"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=2, _
           useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        ''' <summary>
        ''' Fields
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        <ormObjectEntry(typeid:=otDataType.Text, isnullable:=True,
             Title:="Description", Description:="Description of XChange Configuration")>
        Public Const constFNDesc = "desc"

        <ormObjectEntry(typeid:=otDataType.Memo, isnullable:=True,
             Title:="Comments", Description:="Comments")> Public Const constFNTitle = "cmt"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False,
             Title:="IsDynamic", Description:="the XChange Config accepts dynamic addition of XChangeIDs")> Public Const constFNDynamic = "isdynamic"

        <ormObjectEntry(referenceObjectEntry:=XOutline.constobjectid & "." & XOutline.constFNID, isnullable:=True, _
               Title:="Outline ID", Description:="ID to the associated Outline")> Public Const constFNOutline = "outline"

        <ormObjectEntry(typeid:=otDataType.Text, size:=255, isnullable:=True,
              Title:="Message Log Tag", Description:="Message Log Tag")> Public Const constFNMsgLogTag = "msglogtag"


        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>
        <ormEntryMapping(EntryName:=constFNID)> Private _configname As String = ""
        <ormEntryMapping(EntryName:=constFNDesc)> Private _description As String
        <ormEntryMapping(EntryName:=constFNMsgLogTag)> Private _msglogtag As String
        <ormEntryMapping(EntryName:=constFNDynamic)> Private _DynamicAttributes As Boolean
        <ormEntryMapping(EntryName:=constFNOutline)> Private _outlineid As String

        ''' <summary>
        ''' Relations ! BEWARE HARDCODED Typeids
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        '''
        '*** relation to xconfig object entries
        <ormSchemaRelation(linkobject:=GetType(XChangeObjectEntry), cascadeOnDelete:=True, cascadeOnUpdate:=True, _
            fromEntries:={constFNID}, toEntries:={XChangeObjectEntry.ConstFNXConfigID}, _
            linkjoin:=" AND [" & XChangeObjectEntry.ConstFNTypeid & "] ='ObjectEntry'")> _
        Public Const ConstRObjectEntries = "XCHANGEENTRIES"

        <ormEntryMapping(RelationName:=ConstRObjectEntries, infuseMode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand, _
            keyentries:={XChangeObjectEntry.constFNIDNo})> Private WithEvents _ObjectEntryCollection As New ormRelationCollection(Of XChangeObjectEntry)(Me, {XChangeConfigAbstractEntry.constFNIDNo})

        '*** relation xconfig objects
        <ormSchemaRelation(linkobject:=GetType(XChangeObject), cascadeOnDelete:=True, cascadeOnUpdate:=True, _
           fromEntries:={constFNID}, toEntries:={XChangeObject.ConstFNXConfigID}, _
           linkjoin:=" AND [" & XChangeObject.ConstFNTypeid & "] ='Object'")> Public Const ConstRObjects = "XCHANGEOBJECTS"

        <ormEntryMapping(RelationName:=ConstRObjects, infuseMode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand, _
            keyentries:={XChangeObject.constFNIDNo})> _
        Private WithEvents _ObjectCollection As New ormRelationCollection(Of XChangeObject)(Me, {XChangeObject.constFNIDNo})


        ''' <summary>
        '''  dynamic entries
        ''' </summary>
        ''' <remarks></remarks>
        Private _msglog As New ObjectLog
        Private _processedDate As Date = constNullDate

        ' members itself per key:=indexnumber, item:=IXChangeConfigEntry
        'Private _members As New SortedDictionary(Of Long, IXChangeConfigEntry)
        Private _entriesByordinal As New SortedDictionary(Of Ordinal, List(Of IXChangeConfigEntry))

        ' reference object order list to work through members in the row of the exchange
        Private _ObjectDictionary As New Dictionary(Of String, XChangeObject)
        Private _objectsByOrderDirectory As New SortedDictionary(Of Long, XChangeObject)

        ' reference Attributes list to work
        Private _entriesXIDDirectory As New Dictionary(Of String, XChangeObjectEntry)
        Private _entriesByObjectnameDirectory As New Dictionary(Of String, List(Of XChangeObjectEntry))
        Private _entriesXIDList As New Dictionary(Of String, List(Of XChangeObjectEntry)) ' list if IDs are not unique
        Private _aliasDirectory As New Dictionary(Of String, List(Of XChangeObjectEntry))

        ' object ordinalMember -> Members which are driving the ordinal of the complete eXchange
        ' Private _orderByMembers As New Dictionary(Of Object, IXChangeConfigEntry)

        '** dynamic outline
        Dim _outline As New XOutline



#Region "Properties"


        ''' <summary>
        ''' Gets or sets the S outlineid.
        ''' </summary>
        ''' <value>The S outlineid.</value>
        Public Property OutlineID() As String
            Get
                If _outlineid Is Nothing Then Return ""
                Return Me._outlineid
            End Get
            Set(value As String)
                SetValue(constFNOutline, value)
            End Set
        End Property
        ReadOnly Property Outline As XOutline
            Get
                If Me._outlineid <> "" And (Me.IsLoaded Or Me.IsCreated) Then
                    If Not _outline.IsLoaded And Not _outline.IsCreated Then
                        If _outline.Inject(Me._outlineid) Then
                            Return _outline
                        Else
                            _outline = New XOutline
                            Return Nothing
                        End If
                    Else
                        Return _outline
                    End If
                Else
                    _outline = New XOutline
                    Return Nothing
                End If
            End Get

        End Property


        ''' <summary>
        ''' Gets or sets the dynamic attributes.
        ''' </summary>
        ''' <value>The S dynamic attributes.</value>
        Public Property AllowDynamicAttributes() As Boolean
            Get
                Return Me._DynamicAttributes
            End Get
            Set(value As Boolean)
                SetValue(constFNDynamic, value)
            End Set
        End Property

        '****** getUniqueTag
        Public Function GetUniqueTag()
            GetUniqueTag = ConstDelimiter & constTableID & ConstDelimiter & _configname & ConstDelimiter & "0" & ConstDelimiter
        End Function
        ReadOnly Property Msglogtag() As String
            Get
                If _msglogtag Is Nothing Then
                    _msglogtag = GetUniqueTag()
                End If
                Msglogtag = _msglogtag
            End Get

        End Property
        ''' <summary>
        ''' gets name of configuration
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Configname()
            Get
                Configname = _configname
            End Get

        End Property
        ''' <summary>
        ''' gets or sets the description
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Return _description
            End Get
            Set(value As String)
                SetValue(constFNDesc, value)
            End Set
        End Property
        ''' <summary>
        ''' sets the dynamic processed date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProcessedDate() As Date
            Get
                ProcessedDate = _processedDate
            End Get
            Set(value As Date)
                _processedDate = value
            End Set
        End Property

        ''' <summary>
        ''' get the number of objects
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property NoObjects() As Long
            Get
                Return _ObjectCollection.Count
            End Get
        End Property
        ''' <summary>
        ''' get the number of entries
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property NoObjectEntries() As Long
            Get
                Return _ObjectEntryCollection.Count
            End Get
        End Property

#End Region


        ''' <summary>
        '''  get the maximal ordinal of exchange object entry as long if it is numeric
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMaxordinalNo() As Long
            If Not IsAlive(subname:="GetmaxordinalNo") Then Return 0
            Return _entriesByordinal.Keys.Select(Function(x) CLng(x.Value)).Max()
        End Function

        ''' <summary>
        ''' returns the maximal index number of a xchange object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMaxObjectIDNO() As Long

            If NoObjects > 0 Then
                Return Me.ObjectIDNos.Max
            Else
                Return 0
            End If

        End Function
        ''' <summary>
        ''' returns the maximal index number of a xchange entry
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMaxObjectEntryIDNO() As Long

            If NoObjectEntries > 0 Then
                Return Me.ObjectEntryIDNos.Max
            Else
                Return 0
            End If

        End Function


        ''' <summary>
        ''' gets the highest XCommand Ranking
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetHighestXCmd() As otXChangeCommandType

            Dim aHighestXcmd As otXChangeCommandType

            aHighestXcmd = 0

            Dim listofObjects As List(Of XChangeObject) = Me.[XChangeobjects]
            If listofObjects.Count = 0 Then
                Return 0
            End If

            For Each aChangeMember As XChangeObject In listofObjects
                'aChangeMember = m
                Select Case aChangeMember.XChangeCmd
                    Case otXChangeCommandType.Read
                        If aHighestXcmd = 0 Then
                            aHighestXcmd = aChangeMember.XChangeCmd
                        Else
                            'aHighestXcmd = aChangeMember.xChangeCmd
                        End If

                    Case otXChangeCommandType.Update
                        If aHighestXcmd = 0 Or aHighestXcmd = otXChangeCommandType.Read Then
                            aHighestXcmd = aChangeMember.XChangeCmd
                        Else
                            'aHighestXcmd = aChangeMember.xChangeCmd
                        End If
                    Case otXChangeCommandType.UpdateCreate
                        If aHighestXcmd = 0 Or aHighestXcmd = otXChangeCommandType.Read Or aHighestXcmd = otXChangeCommandType.UpdateCreate Then
                            aHighestXcmd = aChangeMember.XChangeCmd
                        Else
                            'aHighestXcmd = aChangeMember.xChangeCmd

                        End If
                End Select
            Next

            Return aHighestXcmd
        End Function

        '*** get the highest need XCMD to run the attributes XCMD
        '***
        Public Function GetHighestObjectXCmd(ByVal objectname As String) As otXChangeCommandType

            Dim aHighestXcmd As otXChangeCommandType

            aHighestXcmd = 0

            Dim listofAttributes As List(Of XChangeObjectEntry) = Me.GetObjectEntries(objectname:=objectname)
            If listofAttributes.Count = 0 Then
                Return 0
            End If

            For Each aChangeMember As XChangeObjectEntry In listofAttributes
                'aChangeMember = m
                Select Case aChangeMember.XChangeCmd
                    Case otXChangeCommandType.Read
                        If aHighestXcmd = 0 Then
                            aHighestXcmd = aChangeMember.XChangeCmd
                        Else
                            'aHighestXcmd = aChangeMember.xChangeCmd
                        End If

                    Case otXChangeCommandType.Update
                        If aHighestXcmd = 0 Or aHighestXcmd = otXChangeCommandType.Read Then
                            aHighestXcmd = aChangeMember.XChangeCmd
                        Else
                            'aHighestXcmd = aChangeMember.xChangeCmd
                        End If
                    Case otXChangeCommandType.UpdateCreate
                        If aHighestXcmd = 0 Or aHighestXcmd = otXChangeCommandType.Read Or aHighestXcmd = otXChangeCommandType.UpdateCreate Then
                            aHighestXcmd = aChangeMember.XChangeCmd
                        Else
                            'aHighestXcmd = aChangeMember.xChangeCmd

                        End If
                End Select
            Next

            Return aHighestXcmd
        End Function
        '*** set the ordinal for a given ID
        ''' <summary>
        ''' sets the ordinal for an ID
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <param name="ordinal"></param>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        Public Function SetOrdinalForXID(ByVal XID As String, ByVal ordinal As Object, Optional ByVal objectname As String = "") As Boolean
            Dim anEntry As New XChangeObjectEntry
            ' Nothing
            If Not IsAlive("setOrdinalForXid") Then Return False


            ' get the entry
            anEntry = Me.GetEntryByXID(XID, objectname)
            If anEntry Is Nothing Then
                Return False
            ElseIf Not anEntry.IsLoaded And Not anEntry.IsCreated Then
                Return False
            End If

            If Not TypeOf ordinal Is OnTrack.Database.Ordinal Then
                ordinal = New Ordinal(ordinal)
            End If
            anEntry.Ordinal = ordinal
            AddOrdinalReference(anEntry)
            Return True
        End Function
        '*** set objectXCmd set the maximum XCMD
        ''' <summary>
        ''' set the object xchange command
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="xchangecommand"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetObjectXCmd(ByVal name As String,
                                      ByVal xchangecommand As otXChangeCommandType) As Boolean
            Dim aMember As New XChangeObject

            ' Nothing
            If Not Me.IsLoaded And Not Me.IsCreated Then
                SetObjectXCmd = False
                Exit Function
            End If

            ' return if exists
            If Not _ObjectCollection.ContainsKey(key:=name) Then
                SetObjectXCmd = False
                Exit Function
            Else
                aMember = _ObjectCollection.Item(key:=name)
                ' depending what the current object xcmd, set it to "max" operation
                Select Case aMember.XChangeCmd

                    Case otXChangeCommandType.Update
                        If xchangecommand <> otXChangeCommandType.Read Then
                            aMember.XChangeCmd = xchangecommand
                        End If
                    Case otXChangeCommandType.Delete
                        ' keep it
                    Case otXChangeCommandType.UpdateCreate
                        If xchangecommand <> otXChangeCommandType.Read And xchangecommand <> otXChangeCommandType.Update Then
                            aMember.XChangeCmd = xchangecommand
                        End If
                    Case otXChangeCommandType.Duplicate
                        ' keep it
                    Case otXChangeCommandType.Read
                        aMember.XChangeCmd = xchangecommand
                End Select

            End If
            SetObjectXCmd = True
        End Function
        '*** add an Object by Name
        '***
        ''' <summary>
        ''' Adds an object to exchange by name and orderno
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="orderno"></param>
        ''' <param name="xcmd"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddObjectByName(ByVal name As String,
                                        Optional ByVal orderno As Long = 0,
                                        Optional ByVal xcmd As otXChangeCommandType = 0) As Boolean

            Dim aXchangeObject As New XChangeObject
            Dim anObjectDef As ObjectDefinition = CurrentSession.Objects.GetObject(name)
            name = name.ToUpper
            If xcmd = 0 Then xcmd = otXChangeCommandType.Read

            ' Nothing
            If Not Me.IsAlive(subname:="AddObjectByName") Then
                Return False
            End If

            ' return if exists
            If _ObjectDictionary.ContainsKey(key:=name) Then
                If xcmd = 0 Then
                    aXchangeObject = _ObjectDictionary.Item(key:=name)
                    xcmd = aXchangeObject.XChangeCmd
                End If
                Call SetObjectXCmd(name:=name, xchangecommand:=xcmd)
                Return False
            End If

            ' load
            If anObjectDef Is Nothing Then
                CoreMessageHandler(message:="Object couldnot be retrieved", subname:="XChangeConfiguration.AddObjectByname", messagetype:=otCoreMessageType.InternalError, _
                                    arg1:=name, objectname:=Me.ObjectID)
                Return False
            End If

            ' add 
            aXchangeObject = XChangeObject.Create(Me.Configname, Me.GetMaxObjectIDNO + 1, objectname:=name, xcmd:=xcmd, domainid:=DomainID, runtimeonly:=Me.RunTimeOnly)
            If aXchangeObject IsNot Nothing Then
                _ObjectCollection.Add(aXchangeObject)
                Return True
            End If

            Return False

        End Function

        ''' <summary>
        ''' Adds an xchange entry by object- and entryname
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <param name="ordinal"></param>
        ''' <param name="OBJECTNAME"></param>
        ''' <param name="ISXCHANGED"></param>
        ''' <param name="XCMD"></param>
        ''' <param name="READONLY"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddEntryByObjectEntry(ByRef entryname As String,
                                             ByVal objectname As String,
                                                Optional ByVal ordinal As Object = Nothing,
                                                Optional ByVal isXChanged As Boolean = True,
                                                Optional ByVal xcmd As otXChangeCommandType = 0,
                                                Optional ByVal [readonly] As Boolean = False) As Boolean

            ' Nothing
            If Not IsAlive("AddEntryByObjectEntry") Then Return False
            Dim anObjectEntry As iormObjectEntry = CurrentSession.Objects.GetEntry(objectname:=objectname, entryname:=entryname)
            entryname = entryname.ToUpper
            objectname = objectname.ToUpper
            If xcmd = 0 Then xcmd = otXChangeCommandType.Read


            If Not anObjectEntry Is Nothing Then
                Return Me.AddEntryByObjectEntry(objectentry:=anObjectEntry, objectname:=objectname, ordinal:=ordinal, isxchanged:=isXChanged, xcmd:=xcmd, [readonly]:=[readonly])
            Else
                Call CoreMessageHandler(message:="field entry not found", arg1:=objectname & "." & entryname, messagetype:=otCoreMessageType.InternalError,
                                         subname:="XChangeConfiguration.addAttributeByField")

                Return False
            End If

        End Function
        ''' <summary>
        ''' adds an xchange entry by the objectentry from the repository
        ''' </summary>
        ''' <param name="objectentry"></param>
        ''' <param name="ordinal"></param>
        ''' <param name="objectname"></param>
        ''' <param name="isxchanged"></param>
        ''' <param name="xcmd"></param>
        ''' <param name="readonly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddEntryByObjectEntry(ByRef objectentry As iormObjectEntry,
                                        Optional ByVal ordinal As Object = Nothing,
                                        Optional ByVal objectname As String = "",
                                        Optional ByVal isxchanged As Boolean = True,
                                        Optional ByVal xcmd As otXChangeCommandType = 0,
                                        Optional ByVal [readonly] As Boolean = False) As Boolean
            Dim anEntry As XChangeObjectEntry
            Dim aVAlue As Object
            Dim aXchangeObject As XChangeObject
            objectname = objectname.ToUpper


            ' isalive
            If Not Me.IsAlive(subname:="AddEntryByObjectEntry") Then Return False

            ' if ordinal is missing -> create one
            If ordinal Is Nothing Then
                For Each [alias] In objectentry.Aliases
                    'could be more than one Attribute by Alias
                    anEntry = Me.GetEntryByXID(XID:=[alias])
                    If anEntry IsNot Nothing Then
                        If anEntry.IsLoaded Or anEntry.IsCreated Then
                            ordinal = anEntry.Ordinal
                            Exit For
                        End If
                    End If
                Next
            End If
            If ordinal Is Nothing Then
                aVAlue = Me.GetMaxordinalNo
                If aVAlue < constXCHCreateordinal - 1 Then
                    ordinal = New Ordinal(constXCHCreateordinal)
                Else
                    ordinal = New Ordinal(aVAlue + 1)
                End If

            End If

            '*** Add the Object if necessary
            If objectname = "" Then
                aXchangeObject = Me.GetObjectByName(objectentry.Objectname)
                If aXchangeObject Is Nothing Then
                    If Me.AddObjectByName(name:=objectentry.Objectname, xcmd:=xcmd) Then
                        aXchangeObject = Me.GetObjectByName(objectentry.Objectname)
                    End If
                End If
            Else
                aXchangeObject = Me.GetObjectByName(objectname)
                If aXchangeObject Is Nothing Then
                    If Me.AddObjectByName(name:=objectname, xcmd:=xcmd) Then
                        aXchangeObject = Me.GetObjectByName(objectname)
                    End If
                End If
            End If

            '** add a default command -> might be also 0 if object was added with entry
            If xcmd = 0 Then xcmd = aXchangeObject.XChangeCmd
            If xcmd = 0 Then xcmd = otXChangeCommandType.Read

            ' add the component
            anEntry = XChangeObjectEntry.Create(Me.Configname, Me.GetMaxObjectEntryIDNO + 1)
            If anEntry IsNot Nothing Then
                anEntry.XID = objectentry.XID
                If Not TypeOf ordinal Is OnTrack.Database.Ordinal Then
                    ordinal = New Ordinal(ordinal)
                End If

                anEntry.Ordinal = ordinal ' create an ordinal 
                anEntry.ObjectEntryname = objectentry.Entryname
                anEntry.IsXChanged = isxchanged
                anEntry.IsReadOnly = [readonly]
                'aMember.[ObjectEntryDefinition] = objectentry dynamic
                anEntry.Objectname = aXchangeObject.Objectname
                anEntry.XChangeCmd = xcmd
                ' add the Object too
                _ObjectEntryCollection.Add(anEntry)
                Return True
            End If

            Return False


        End Function
        ''' <summary>
        ''' Adds an Entry  by its XChange-ID
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="ordinal"></param>
        ''' <param name="objectname"></param>
        ''' <param name="isXChanged"></param>
        ''' <param name="xcmd"></param>
        ''' <param name="readonly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddEntryByXID(ByVal Xid As String,
                                            Optional ByVal ordinal As Object = Nothing,
                                            Optional ByVal objectname As String = "",
                                            Optional ByVal isXChanged As Boolean = True,
                                            Optional ByVal xcmd As otXChangeCommandType = Nothing,
                                            Optional ByVal [readonly] As Boolean = False) As Boolean


            AddEntryByXID = False
            objectname = objectname.ToUpper
            Xid = Xid.ToUpper

            ' isalive
            If Not Me.IsAlive(subname:="AddEntryByXID") Then Return False

            '*** no objectname -> get all IDs in objects
            If objectname = "" Then
                For Each entry In CurrentSession.Objects.GetEntryByXID(xid:=Xid)
                    '** compare to objects in order
                    If Me.NoObjects > 0 Then
                        For Each anObjectEntry In Me.ObjectsByOrderNo
                            If entry.Objectname = anObjectEntry.Objectname Then
                                AddEntryByXID = AddEntryByObjectEntry(objectentry:=entry, ordinal:=ordinal,
                                                                  isxchanged:=isXChanged,
                                                                  objectname:=entry.Objectname,
                                                                  xcmd:=xcmd, readonly:=[readonly])
                            End If
                        Next
                        ' simply add

                    Else
                        AddEntryByXID = AddEntryByObjectEntry(objectentry:=entry, ordinal:=ordinal,
                                                          isxchanged:=isXChanged,
                                                          objectname:=entry.Objectname, xcmd:=xcmd, readonly:=[readonly])
                    End If

                Next

            Else
                For Each entry In CurrentSession.Objects.GetEntryByXID(xid:=Xid)
                    If objectname = entry.Objectname Then
                        AddEntryByXID = AddEntryByObjectEntry(objectentry:=entry, ordinal:=ordinal,
                                                          isxchanged:=isXChanged,
                                                          objectname:=entry.Objectname,
                                                          xcmd:=xcmd, readonly:=[readonly])
                    End If
                Next


            End If

            ' return
            AddEntryByXID = AddEntryByXID Or False
            Exit Function


        End Function
        ''' <summary>
        ''' returns True if an Objectname with an ID exists
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Exists(Optional ByVal objectname As String = "", Optional ByVal XID As String = "") As Boolean
            Dim flag As Boolean
            objectname = objectname.ToUpper
            XID = XID.ToUpper

            ' Nothing
            If Not Me.IsLoaded And Not Me.IsCreated Then
                Exists = False
                Exit Function
            End If

            ' missing arguments
            If objectname = "" Then
                Call CoreMessageHandler(subname:="XChangeConfiguration.exists", message:="objectname was not set", _
                                        messagetype:=otCoreMessageType.InternalError)
                Exists = False
                Exit Function
            End If
            ' missing arguments
            If objectname = "" And XID = "" Then
                Call CoreMessageHandler(subname:="XChangeConfiguration.exists", message:="set either objectname or attributename - not both", _
                                        messagetype:=otCoreMessageType.InternalError)
                Exists = False
                Exit Function
            End If

            '+ check
            If objectname <> "" And XID = "" Then
                If _ObjectCollection.ContainsKey(key:=objectname) Then
                    Exists = True
                Else
                    Exists = False
                End If
                Exit Function
            Else
                If _entriesXIDDirectory.ContainsKey(key:=XID) Then

                    Exists = True
                Else
                    Exists = False
                End If
                Exit Function
            End If
        End Function

        ''' <summary>
        ''' Add an Entry by ID
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AddXIDReference(ByRef entry As XChangeObjectEntry) As Boolean
            Dim entries As List(Of XChangeObjectEntry)

            If _entriesXIDList.ContainsKey(key:=UCase(entry.XID)) Then
                entries = _entriesXIDList.Item(UCase(entry.XID))
            Else

                entries = New List(Of XChangeObjectEntry)
                _entriesXIDList.Add(UCase(entry.XID), entries)
            End If
            If entries.Contains(entry) Then entries.Remove(entry)
            entries.Add(entry)

            Return True
        End Function
        ''' <summary>
        ''' Add ordinal to Reference Structures
        ''' </summary>
        ''' <param name="member"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AddOrdinalReference(ByRef entry As IXChangeConfigEntry) As Boolean
            Dim entries As List(Of IXChangeConfigEntry)
            '** sorted
            If _entriesByordinal.ContainsKey(key:=entry.Ordinal) Then
                entries = _entriesByordinal.Item(entry.Ordinal)
            Else
                entries = New List(Of IXChangeConfigEntry)
                _entriesByordinal.Add(entry.Ordinal, entries)
            End If

            If entries.Contains(entry) Then entries.Remove(entry)
            entries.Add(entry)

            Return True
        End Function
        ''' <summary>
        ''' Add an Entry
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AddObjectReference(ByRef entry As XChangeObjectEntry) As Boolean
            Dim entries As List(Of XChangeObjectEntry)

            If _entriesByObjectnameDirectory.ContainsKey(key:=entry.Objectname) Then
                entries = _entriesByObjectnameDirectory.Item(entry.Objectname)
            Else
                entries = New List(Of XChangeObjectEntry)
                _entriesByObjectnameDirectory.Add(entry.Objectname, entries)
            End If

            entries.Add(entry)

            Return True
        End Function
        ''' <summary>
        ''' Add an Entry by ID
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AddAliasReference(ByRef Entry As XChangeObjectEntry) As Boolean
            Dim entries As List(Of XChangeObjectEntry)

            For Each [alias] As String In Entry.Aliases

                If _aliasDirectory.ContainsKey(key:=UCase([alias])) Then
                    entries = _aliasDirectory.Item(key:=UCase([alias]))
                Else
                    entries = New List(Of XChangeObjectEntry)
                    _aliasDirectory.Add(key:=UCase([alias]), value:=entries)
                End If
                If entries.Contains(Entry) Then entries.Remove(Entry)
                entries.Add(Entry)
            Next

            Return True
        End Function
        ''' <summary>
        ''' Event Handler for on Removed
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub XChangeConfiguration_OnRemovedEntry(sender As Object, e As ormRelationCollection(Of XChangeObjectEntry).EventArgs) Handles _ObjectEntryCollection.OnRemoved
            Dim anEntry = e.Dataobject
            Dim anObjectEntry As XChangeObject

            Throw New NotImplementedException

        End Sub

        ''' <summary>
        ''' Event handler for the Added Entry in the Entries Collection
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub XChangeConfiguration_OnAddEntry(sender As Object, e As ormRelationCollection(Of XChangeObjectEntry).EventArgs) Handles _ObjectEntryCollection.OnAdded
            Dim anEntry As XChangeObjectEntry = e.Dataobject

            ' check on the Object of the Attribute
            If Not _ObjectDictionary.ContainsKey(key:=anEntry.Objectname.ToUpper) Then
                Me.AddObjectByName(anEntry.Objectname.ToUpper)
            End If

            ' add the Attribute
            If _entriesXIDDirectory.ContainsKey(key:=anEntry.XID) Then
                Call _entriesXIDDirectory.Remove(key:=anEntry.XID)
            End If

            Call _entriesXIDDirectory.Add(key:=anEntry.XID, value:=anEntry)
            '** references
            AddXIDReference(anEntry) '-> List references if multipe
            AddObjectReference(anEntry)
            AddAliasReference(anEntry)
            AddOrdinalReference(anEntry)



        End Sub
        ''' <summary>
        ''' Event handler for the Added Entry in the Entries Collection
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub XChangeConfiguration_OnAddEntry(sender As Object, e As ormRelationCollection(Of XChangeObject).EventArgs) Handles _ObjectCollection.OnAdded
            Dim anXchangeObject As XChangeObject = e.Dataobject
            If anXchangeObject Is Nothing Then
                CoreMessageHandler(message:="anEntry is not an ObjectEntry", messagetype:=otCoreMessageType.InternalError,
                                    subname:="XConfig.Addmember")
                Return
            End If

            If _ObjectDictionary.ContainsKey(key:=anXchangeObject.Objectname) Then
                Call _ObjectDictionary.Remove(key:=anXchangeObject.Objectname)
            End If
            Call _ObjectDictionary.Add(key:=anXchangeObject.Objectname, value:=anXchangeObject)
            '**
            If _objectsByOrderDirectory.ContainsKey(key:=anXchangeObject.Orderno) Then
                Call _objectsByOrderDirectory.Remove(key:=anXchangeObject.Orderno)
            End If
            Call _objectsByOrderDirectory.Add(key:=anXchangeObject.Orderno, value:=anXchangeObject)

        End Sub

        ''' <summary>
        ''' Event Handler for on Removed
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub XChangeConfiguration_OnRemovedEntry(sender As Object, e As ormRelationCollection(Of XChangeObject).EventArgs) Handles _ObjectCollection.OnRemoved
            Dim anEntry = e.Dataobject


            Throw New NotImplementedException

        End Sub
        ''' <summary>
        ''' Add XChangeMember
        ''' </summary>
        ''' <param name="anEntry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddEntry(anEntry As XChangeObjectEntry) As Boolean
            ' remove and overwrite
            If _ObjectEntryCollection.Contains(anEntry) Then
                Call _ObjectEntryCollection.Remove(anEntry)
            End If

            ' add Member Entry
            _ObjectEntryCollection.Add(anEntry)
            Return True
        End Function
        ''' <summary>
        ''' resets the object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function Reset() As Boolean
            _ObjectCollection.Clear()
            _objectsByOrderDirectory.Clear()
            _entriesXIDDirectory.Clear()
            _entriesByObjectnameDirectory.Clear()
            _entriesXIDList.Clear()
            _aliasDirectory.Clear()
            _entriesByordinal.Clear()
        End Function


        ''' <summary>
        ''' retrieves an Object by its name or nothing
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectByName(ByVal objectname As String) As XChangeObject

            If _ObjectDictionary.ContainsKey(objectname.ToUpper) Then
                Return _ObjectDictionary.Item(key:=objectname.ToUpper)
            Else
                Return Nothing
            End If

        End Function
        ''' <summary>
        ''' returns the xchange object entry id's
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectEntryIDNos() As IEnumerable(Of Long)
            Get
                Dim alist As New List(Of Long)
                For Each akey In _ObjectEntryCollection.Keys
                    alist.Add(akey(0))
                Next
                Return alist
            End Get
        End Property
        ''' <summary>
        ''' returns the xchange object id's
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectIDNos() As IEnumerable(Of Long)
            Get

                Dim alist As New List(Of Long)
                For Each akey In _ObjectCollection.Keys
                    alist.Add(akey(0))
                Next
                Return alist
            End Get
        End Property


        ''' <summary>
        ''' retrieves the ordinal numbers of the objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectsByOrderNo() As IEnumerable(Of XChangeObject)
            Get
                Return _objectsByOrderDirectory.Values
            End Get
        End Property

        ''' <summary>
        ''' retrieves a List of Attributes per Objectname
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntriesByObjectName(ByVal objectname As String) As IList(Of XChangeObjectEntry)

            If _entriesByObjectnameDirectory.ContainsKey(objectname) Then
                Return _entriesByObjectnameDirectory.Item(key:=objectname)
            Else
                Return New List(Of XChangeObjectEntry)
            End If


        End Function

        ''' <summary>
        ''' gets an relational collection of xchange obejct entries
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property XChangeObjectEntries() As iormRelationalCollection(Of XChangeObjectEntry)
            Get
                Return _ObjectEntryCollection
            End Get
        End Property

        ''' <summary>
        ''' gets an relational collection of the xchange objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property XChangeobjects() As iormRelationalCollection(Of XChangeObject)
            Get
                Return _ObjectCollection
            End Get
        End Property

        ''' <summary>
        ''' returns an attribute by its entryname and objectname
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntryByObjectEntryName(ByVal entryname As String,
                                                    Optional ByVal objectname As String = "") As XChangeObjectEntry

            Dim anEntry As XChangeObjectEntry
            If Not IsAlive(subname:="GetEntryByObjectEntryName") Then Return Nothing
            objectname = objectname.ToUpper
            entryname = entryname.ToUpper

            Dim alist As List(Of XChangeObjectEntry)
            If objectname <> "" Then
                '* might be we have the object but no fields
                If _entriesByObjectnameDirectory.ContainsKey(key:=objectname) Then
                    alist = _entriesByObjectnameDirectory.Item(key:=objectname)
                    anEntry = alist.Find(Function(m As XChangeObjectEntry)
                                             Return m.ObjectEntryname = entryname
                                         End Function)

                    If Not anEntry Is Nothing Then
                        Return anEntry
                    End If
                End If

            Else
                For Each objectdef In _objectsByOrderDirectory.Values
                    If _entriesByObjectnameDirectory.ContainsKey(key:=objectdef.Objectname) Then
                        alist = _entriesByObjectnameDirectory(key:=objectdef.Objectname)

                        anEntry = alist.Find(Function(m As XChangeObjectEntry)
                                                 Return m.ObjectEntryname = entryname
                                             End Function)

                        If Not anEntry Is Nothing Then
                            Return anEntry
                        End If
                    End If
                Next
            End If

            '** search also by ID and consequent by ALIAS
            Dim anObjectEntry As iormObjectEntry = CurrentSession.Objects.GetEntry(objectname:=objectname, entryname:=entryname)
            If Not anObjectEntry Is Nothing AndAlso anObjectEntry.XID IsNot Nothing Then
                anEntry = Me.GetEntryByXID(XID:=anObjectEntry.XID, objectname:=objectname)
                If Not anEntry Is Nothing Then
                    Return anEntry
                End If
            End If


            Return Nothing
        End Function
        ''' <summary>
        ''' returns an Attribute in the XChange Config by its XChange ID or Alias
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntryByXID(ByVal XID As String, _
                                        Optional ByVal objectname As String = "") As XChangeObjectEntry

            Dim aCollection As IEnumerable
            XID = XID.ToUpper
            objectname = objectname.ToUpper

            If Not Me.IsAlive(subname:="GetEntryByXID") Then
                Return Nothing
            End If

            If _entriesXIDList.ContainsKey(UCase(XID)) Then
                aCollection = _entriesXIDList.Item(UCase(XID))
                For Each entry As XChangeObjectEntry In aCollection
                    If objectname <> "" AndAlso entry.Objectname = objectname Then
                        Return entry
                    ElseIf objectname = "" Then
                        Return entry
                    End If
                Next

            End If

            '** look into aliases 
            '**
            '* check if ID is an ID already in the xconfig
            GetEntryByXID = GetEntrybyAlias(XID, objectname)
            If GetEntryByXID Is Nothing Then
                '* check all Objects coming through with this ID
                For Each anObjectEntry In CurrentSession.Objects.GetEntryByXID(xid:=XID)
                    '** check on all the XConfig Objects
                    For Each anObjectMember In Me.ObjectsByOrderNo
                        '* if ID is included as Alias Name
                        GetEntryByXID = GetEntrybyAlias(alias:=anObjectEntry.XID, objectname:=anObjectMember.Objectname)
                        '** or the aliases are included in this XConfig
                        If GetEntryByXID Is Nothing Then
                            For Each aliasID In anObjectEntry.Aliases
                                GetEntryByXID = GetEntrybyAlias(alias:=aliasID, objectname:=anObjectMember.Objectname)
                                '* found
                                If Not GetEntryByXID Is Nothing Then
                                    Exit For
                                End If
                            Next

                        End If
                        '* found
                        If Not GetEntryByXID Is Nothing Then
                            Exit For
                        End If
                    Next
                    '* found
                    If Not GetEntryByXID Is Nothing Then
                        Exit For
                    End If
                Next

            End If
            Return GetEntryByXID
        End Function
        ''' <summary>
        ''' returns a List of XConfigMembers per ordinal
        ''' </summary>
        ''' <param name="ordinal"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntriesByMappingOrdinal(ByVal ordinal As Ordinal) As List(Of IXChangeConfigEntry)

            If Not Me.IsCreated And Not Me.IsLoaded Then
                Return New List(Of IXChangeConfigEntry)
            End If

            If _entriesByordinal.ContainsKey(ordinal) Then
                Return _entriesByordinal.Item(ordinal)
            Else
                Return New List(Of IXChangeConfigEntry)
            End If

        End Function
        ''' <summary>
        ''' returns an Attribute by its XChange Alias ID
        ''' </summary>
        ''' <param name="alias"></param>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntrybyAlias(ByVal [alias] As String,
                                        Optional ByVal objectname As String = "") As XChangeObjectEntry

            Dim aCollection As IEnumerable
            objectname = objectname.ToUpper

            If Not Me.IsCreated And Not Me.IsLoaded Then
                GetEntrybyAlias = Nothing
                Exit Function
            End If

            If _aliasDirectory.ContainsKey(UCase([alias])) Then

                aCollection = _aliasDirectory.Item(UCase([alias]))
                For Each entry As XChangeObjectEntry In aCollection
                    If objectname <> "" AndAlso entry.Objectname = objectname Then
                        Return entry
                    ElseIf objectname = "" Then
                        Return entry
                    End If
                Next

            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' Returns an ienumerable of all entries (optional just by an objectname)
        ''' </summary>
        ''' <param name="objectname">optional objectname</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectEntries(Optional objectname As String = "") As IEnumerable(Of XChangeObjectEntry)
            If Not IsAlive(subname:="GetObjectEntries") Then Return New List(Of IXChangeConfigEntry)

            If objectname <> "" Then
                Return GetEntriesByObjectName(objectname)
            Else
                Return _entriesXIDDirectory.Values.ToList
            End If

        End Function
        ''' <summary>
        ''' Loads a XChange Configuration from Store
        ''' </summary>
        ''' <param name="configname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal configname As String, _
                                                  Optional domainid As String = "", _
                                                  Optional runtimeonly As Boolean = False) As XChangeConfiguration

            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim primarykey() As Object = {configname.ToUpper, domainid}
            Return ormDataObject.Retrieve(Of XChangeConfiguration)(primarykey, runtimeOnly:=runtimeonly)
        End Function


        ''' <summary>
        ''' creates a persistable object with primary key
        ''' </summary>
        ''' <param name="configname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal configname As String, Optional domainid As String = "", Optional runtimeonly As Boolean = False) As XChangeConfiguration
            Dim primarykey() As Object = {configname.ToUpper, domainid}
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.CreateDataObject(Of XChangeConfiguration)(primarykey, checkUnique:=True)
        End Function


        ''' <summary>
        ''' retrieves a List of all XConfigs
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function All() As List(Of XChangeConfiguration)
            Return ormDataObject.AllDataObject(Of XChangeConfiguration)()
        End Function
    End Class

    ''' <summary>
    ''' describes a XChange Outline data structure
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(ID:=XOutline.constobjectid, version:=1, usecache:=True, adddeletefieldbehavior:=True, adddomainbehavior:=True, _
        modulename:=ConstModuleXChange, description:="describes a XChange Outline data structure")> _
    Public Class XOutline
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable
        Implements IEnumerable(Of XOutlineItem)

        Public Const constobjectid = "XOutline"

        <ormSchemaTableAttribute(Version:=1)> Public Const constTableID = "tblXOutlines"

        ''' <summary>
        ''' Keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(XID:="otl1", primaryKeyordinal:=1, typeid:=otDataType.Text, size:=50,
                    properties:={ObjectEntryProperty.Keyword}, _
                description:="identifier of the outline", Title:="ID")> Public Const constFNID = "id"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=2, _
                        useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNDomainID = Domain.ConstFNDomainID
        ''' <summary>
        '''  Fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(XID:="otl2", typeid:=otDataType.Text, isnullable:=True, _
                description:="description of the outline", Title:="description")> Public Const constFNdesc = "desc"
        <ormObjectEntry(XID:="otl3", typeid:=otDataType.Bool, defaultvalue:=False,
                        description:="True if deliverable revisions are added dynamically", Title:="DynRev")> Public Const constFNRev = "addrev"


        ' key
        <ormEntryMapping(EntryName:=constFNID)> Private _id As String = ""
        <ormEntryMapping(EntryName:=constFNdesc)> Private _desc As String
        <ormEntryMapping(EntryName:=constFNRev)> Private _DynamicAddRevisions As Boolean


        ' components itself per key:=posno, item:=cmid
        Private s_cmids As New OrderedDictionary()


        '** initialize
        Public Sub New()
            Call MyBase.New(constTableID)

        End Sub

#Region "Properties"


        ''' <summary>
        ''' Gets or sets the desc.
        ''' </summary>
        ''' <value>The desc.</value>
        Public Property Description() As String
            Get
                Return Me._desc
            End Get
            Set(value As String)
                Me._desc = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the dynamic add revisions.
        ''' </summary>
        ''' <value>The dynamic add revisions.</value>
        Public Property DynamicAddRevisions() As Boolean
            Get
                Return Me._DynamicAddRevisions
            End Get
            Set(value As Boolean)
                Me._DynamicAddRevisions = value
            End Set
        End Property

        ''' <summary>
        ''' gets the ID of the Outline
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property id()
            Get
                id = _id
            End Get

        End Property
        ''' <summary>
        ''' gets the number outline items in the outline
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Count() As Long
            Get
                Count = s_cmids.Count - 1
            End Get

        End Property
#End Region

        ''' <summary>
        ''' returns the maximal ordinal of the outline items
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMaxordinal() As Ordinal
            Dim keys() As Object
            Dim i As Integer
            Dim max As Ordinal

            If Count >= 0 Then
                For Each pos As Ordinal In s_cmids.Keys
                    If pos > max Then max = pos
                Next
                'keys = s_cmids.Keys
                'For i = LBound(keys) To UBound(keys)
                'If keys(i) > max Then max = keys(i)
                'Next i
                GetMaxordinal = max
            Else
                GetMaxordinal = New Ordinal(0)
            End If
        End Function

        '*** add a Component by cls OTDB
        '***
        Public Function AddOutlineItem(anEntry As XOutlineItem) As Boolean
            Dim flag As Boolean
            Dim existEntry As New XOutlineItem
            Dim m As Object

            ' empty
            If Not Me.IsLoaded And Not Me.IsCreated Then
                AddOutlineItem = False
                Exit Function
            End If

            ' remove and overwrite
            If s_cmids.Contains(key:=anEntry.ordinal) Then
                Call s_cmids.Remove(key:=anEntry.ordinal)
            End If
            ' add entry
            s_cmids.Add(key:=anEntry.ordinal, value:=anEntry)

            '
            AddOutlineItem = True

        End Function
        ''' <summary>
        ''' Initializes the data obejct
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Initialize() As Boolean
            _IsInitialized = MyBase.Initialize
            s_cmids = New OrderedDictionary()

            Return _IsInitialized
        End Function

        ''' <summary>
        ''' ordinals of the components
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ordinals() As Collection

            If Not Me.IsCreated And Not Me.IsLoaded Then
                Return Nothing
            End If

            ordinals = s_cmids.Keys
        End Function
        ''' <summary>
        ''' retrieves a collection of Outline Items
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Items() As Collection
            Dim anEntry As New XOutlineItem
            Dim aCollection As New Collection
            Dim m As Object

            If Not Me.IsCreated And Not Me.IsLoaded Then
                Items = Nothing
                Exit Function
            End If

            ' delete each entry
            For Each kvp As KeyValuePair(Of Ordinal, XOutlineItem) In s_cmids
                anEntry = kvp.Value
                If anEntry.ordinal <> New Ordinal(0) Then
                    aCollection.Add(anEntry)
                End If
            Next

            Items = aCollection
        End Function

        ''' <summary>
        ''' Create persistable schema for this dataobject
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function CreateSchema() As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of XOutline)()
        End Function
        ''' <summary>
        ''' loads the X Outline from the datastore
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(ByVal id As String) As Boolean
            Dim pkarry() As Object = {id}

            If MyBase.Inject(pkArray:=pkarry) Then
                LoadItems(id:=id)
            End If

            Return Me.IsLoaded
        End Function

        ''' <summary>
        ''' load all the related outline items
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function LoadItems(Optional ByVal id As String = "") As Boolean
            Try
                If id = "" Then id = Me.id

                Dim aCollection As SortedList(Of Ordinal, XOutlineItem) = XOutlineItem.AllByID(id:=id)

                '* add all
                For Each anEntry As XOutlineItem In aCollection.Values
                    If Not Me.AddOutlineItem(anEntry) Then
                        Call CoreMessageHandler(message:="a XOutlineItem couldnot be added to an outline", arg1:=anEntry.ToString,
                                                 entryname:=id, objectname:=constTableID, messagetype:=otCoreMessageType.InternalError,
                                                 subname:="clsOTDBXoutline.loaditems")
                    End If
                Next

                Return True
            Catch ex As Exception

                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBXOutline.loadItems")
                Me.Unload()
                Return False
            End Try
        End Function

        ''' <summary>
        ''' persist the Outline and the components
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional ByVal timestamp As Date = constNullDate) As Boolean
            Try
                Persist = MyBase.Persist(timestamp:=timestamp)
                If Persist Then
                    ' save each entry
                    For Each anEntry As XOutlineItem In s_cmids.Values
                        'Dim anEntry As XOutlineItem = kvp.Value
                        Persist = Persist And anEntry.Persist(timestamp)
                    Next
                End If

                Return Persist
                Exit Function

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBXOutline.persist")
                Return False
            End Try


        End Function
        ''' <summary>
        ''' create an persistable outline
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal id As String) As Boolean
            Dim anEntry As New XOutlineItem
            Dim pkarray() As Object = {id}
            If IsLoaded Then
                Create = False
                Exit Function
            End If

            If MyBase.Create(pkArray:=pkarray, checkUnique:=True) Then
                ' set the primaryKey
                _id = id
            End If

            ' abort create if exists
            Return Me.IsCreated
        End Function

        '*****
        '***** CleanUpRevisions (if dynamic revision than throw out all the revisions)
        Public Function CleanUpRevision() As Boolean

            Dim aDeliverable As New Deliverable
            Dim aFirstRevision As New Deliverable
            Dim deletedColl As New Collection


            If Not CurrentSession.RequireAccessRight(accessRequest:=otAccessRight.ReadUpdateData) Then
                Call CoreMessageHandler(subname:="clsOTDBXOutline.cleanupRevision", message:="Read Update not granted",
                                       messagetype:=otCoreMessageType.ApplicationError)
                Return False
            End If
            If Not Me.IsLoaded And Not Me.IsCreated Then
                Return False
            ElseIf Not Me.DynamicAddRevisions Then
                Return False
            End If

            '*** go through all items in Outline and delete the NON-Firstrevisions 
            '*** without checking if the first revisions are in the outline

            For Each item As XOutlineItem In s_cmids.Values
                Dim keys As List(Of XOutlineItem.OTLineKey) = item.keys

                '** look for Deliverable UID
                For Each key In keys
                    If key.ID.ToLower = "uid" Or key.ID.ToLower = "sc2" Then
                        aFirstRevision = New Deliverable
                        If aFirstRevision.Inject(uid:=CLng(key.Value)) Then
                            If Not aFirstRevision.IsFirstRevision Or aFirstRevision.IsDeleted Then
                                deletedColl.Add(Item:=item)
                                Call item.Delete()
                            End If
                        End If
                    End If
                Next

            Next

            For Each item As XOutlineItem In deletedColl
                s_cmids.Remove(key:=item.ordinal)
            Next

            Call CoreMessageHandler(message:="outline cleaned from revisions", subname:="clsOTDBXoutline.cleanuprevision",
                                         arg1:=Me.id, messagetype:=otCoreMessageType.ApplicationInfo)
            Return True

        End Function

        Public Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Me.GetEnumerator()
        End Function
        '****** Enumerator with dynamic Revisions
        '******
        Public Function GetEnumerator() As IEnumerator(Of XOutlineItem) Implements IEnumerable(Of XOutlineItem).GetEnumerator
            Dim aDeliverable As New Deliverable
            Dim aFirstRevision As New Deliverable
            Dim returnCollection As New List(Of XOutlineItem)

            If Not Me.IsLoaded And Not Me.IsCreated Then
                Return returnCollection
            ElseIf Not Me.DynamicAddRevisions Then
                Return returnCollection
            End If

            '*** go through all items in Outline and delete the NON-Firstrevisions 
            '*** without checking if the first revisions are in the outline

            For Each item As XOutlineItem In s_cmids.Values
                Dim keys As List(Of XOutlineItem.OTLineKey) = item.keys

                '** look for Deliverable UID
                If item.IsText Or item.IsGroup Then
                    returnCollection.Add(item)
                Else
                    For Each key In keys
                        If key.ID.ToLower = "uid" Or key.ID.ToLower = "sc2" Then
                            aFirstRevision = New Deliverable
                            If Me.DynamicAddRevisions AndAlso aFirstRevision.Inject(uid:=CLng(key.Value)) Then
                                If aFirstRevision.IsFirstRevision And Not aFirstRevision.IsDeleted Then
                                    ' add all revisions inclusive the follow ups
                                    For Each uid As Long In Deliverable.AllRevisionUIDsBy(aFirstRevision.Uid)
                                        Dim newKey As New XOutlineItem.OTLineKey(otDataType.[Long], "uid", uid)
                                        Dim newKeylist As New List(Of XOutlineItem.OTLineKey)
                                        newKeylist.Add(newKey)
                                        Dim newOI As New XOutlineItem
                                        newOI.Create(ID:=item.OutlineID, level:=item.Level, ordinal:=item.ordinal)
                                        newOI.keys = newKeylist
                                        newOI.Level = item.Level
                                        newOI.Text = item.Text

                                        returnCollection.Add(newOI)
                                    Next
                                End If
                            Else
                                returnCollection.Add(item)
                            End If
                        Else
                            returnCollection.Add(item)
                        End If
                    Next
                End If


            Next

            Return returnCollection.GetEnumerator
        End Function

    End Class

    ''' <summary>
    ''' OutlineItem of an Outline
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(ID:=XOutlineItem.constObjectID, version:=1, usecache:=True, adddeletefieldbehavior:=True, adddomainbehavior:=True, _
        modulename:=ConstModuleXChange, description:="describes a XChange Outline Item")> _
    Public Class XOutlineItem
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable


        ''' <summary>
        ''' OutlineKey Class as subclass of outline item to make it flexible
        ''' </summary>
        ''' <remarks></remarks>
        Public Class OTLineKey
            Private _Value As Object
            Private _ID As String
            Private [_Type] As otDataType

            Public Sub New(ByVal [Type] As otDataType, ByVal ID As String, ByVal value As Object)
                _Value = value
                _ID = ID
                _Type = [Type]
            End Sub
            ''' <summary>
            ''' Gets the type.
            ''' </summary>
            ''' <value>The type.</value>
            Public ReadOnly Property Type() As otDataType
                Get
                    Return Me.[_Type]
                End Get
            End Property

            ''' <summary>
            ''' Gets the ID.
            ''' </summary>
            ''' <value>The ID.</value>
            Public ReadOnly Property ID() As String
                Get
                    Return Me._ID
                End Get
            End Property

            ''' <summary>
            ''' Gets the value.
            ''' </summary>
            ''' <value>The value.</value>
            Public ReadOnly Property Value() As Object
                Get
                    Return Me._Value
                End Get
            End Property

        End Class

        Public Const constObjectID = "XOutlineItem"
        <ormSchemaTableAttribute(version:=1)> Public Const constTableID = "tblXOutlineItems"
        <ormSchemaIndexAttribute(columnname1:=constFNID, columnname2:=ConstFNordinall)> Public Const constIndexLongOutline = "longOutline"
        <ormSchemaIndexAttribute(columnname1:=ConstFNUid, columnname2:="id", columnname3:=ConstFNordinals)> Public Const constIndexUsedOutline = "UsedOutline"

        ''' <summary>
        ''' keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(XID:="otl1", primaryKeyordinal:=1, referenceObjectEntry:=XOutline.constobjectid & "." & XOutline.constFNID, _
            title:="Outline ID", description:="identifier of the outline")> Public Const constFNID = XOutline.constFNID

        <ormObjectEntry(XID:="otli3", primaryKeyordinal:=2, typeid:=otDataType.Text, size:=255,
         title:="ordinals", description:="ordinal as string of the outline item")> Public Const ConstFNordinals = "ordials"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=3, _
            useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        ''' <summary>
        ''' foreign key
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaForeignKey(useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            entrynames:={constFNID, ConstFNDomainID}, _
            foreignkeyreferences:={XOutline.constobjectid & "." & XOutline.constFNID, _
            XOutline.constobjectid & "." & XOutline.ConstFNDomainID})> Public Const constFKXOUTLINE = "FK_XOUTLINE"

        ''' <summary>
        ''' fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(XID:="otli2", typeid:=otDataType.Long,
           title:="ordinal", description:="ordinal as long of the outline")> Public Const ConstFNordinall = "ordiall"

        <ormObjectEntry(XID:="dlvuid", referenceobjectentry:=Deliverable.ConstObjectID & "." & Deliverable.constFNUid, _
        isnullable:=True, useforeignkey:=otForeignKeyImplementation.NativeDatabase,
         title:="deliverable uid", description:="uid of the deliverable")> Public Const ConstFNUid = Deliverable.constFNUid

        <ormObjectEntry(XID:="otli4", typeid:=otDataType.Long, defaultvalue:=1,
          title:="identlevel", description:="identlevel as string of the outline")> Public Const ConstFNIdent = "level"

        <ormObjectEntry(XID:="otli10", typeid:=otDataType.List, innertypeid:=otDataType.Text,
         title:="Types", description:="types the outline key")> Public Const ConstFNTypes = "types"

        <ormObjectEntry(XID:="otli11", typeid:=otDataType.List, innertypeid:=otDataType.Text,
         title:="IDs", description:="ids the outline key")> Public Const ConstFNIDs = "ids"

        <ormObjectEntry(XID:="otli12", typeid:=otDataType.List, innertypeid:=otDataType.Text,
        title:="Values", description:="values the outline key")> Public Const ConstFNValues = "values"

        <ormObjectEntry(XID:="otli13", typeid:=otDataType.Bool, defaultvalue:=False,
        title:="Grouping Item", description:="check if this an grouping item")> Public Const ConstFNisgroup = "isgrouped"

        <ormObjectEntry(XID:="otli14", typeid:=otDataType.Bool, defaultvalue:=False,
       title:="Text Item", description:="check if this an text item")> Public Const ConstFNisText = "istext"

        <ormObjectEntry(XID:="otli14", typeid:=otDataType.Text, isnullable:=True,
       title:="Text", description:="Text if a text item")> Public Const ConstFNText = "text"

        <ormEntryMapping(EntryName:=constFNID)> Private _id As String = ""   ' ID of the outline

        Private _keys As New List(Of OTLineKey)    'keys and values
        Private _ordinal As Ordinal ' extramapping

        <ormEntryMapping(EntryName:=ConstFNIdent)> Private _level As Long = 0
        <ormEntryMapping(EntryName:=ConstFNisgroup)> Private _isGroup As Boolean
        <ormEntryMapping(EntryName:=ConstFNisText)> Private _isText As Boolean
        <ormEntryMapping(EntryName:=ConstFNText)> Private _text As String = ""

#Region "properties"
        ''' <summary>
        ''' Gets or sets the text.
        ''' </summary>
        ''' <value>The text.</value>
        Public Property Text() As String
            Get
                Return Me._text
            End Get
            Set(value As String)
                Me._text = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the is text.
        ''' </summary>
        ''' <value>The is text.</value>
        Public Property IsText() As Boolean
            Get
                Return Me._isText
            End Get
            Set(value As Boolean)
                Me._isText = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the is group.
        ''' </summary>
        ''' <value>The is group.</value>
        Public Property IsGroup() As Boolean
            Get
                Return Me._isGroup
            End Get
            Set(value As Boolean)
                Me._isGroup = value
            End Set
        End Property

        ReadOnly Property OutlineID() As String
            Get
                OutlineID = _id

            End Get
        End Property

        ReadOnly Property ordinal() As Ordinal
            Get
                ordinal = _ordinal
            End Get

        End Property

        Public Property keys() As List(Of OTLineKey)
            Get
                keys = _keys
            End Get
            Set(value As List(Of OTLineKey))
                _keys = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Level() As UShort
            Get

                Level = _level
            End Get
            Set(value As UShort)
                _level = value
                Me.IsChanged = True
            End Set
        End Property


#End Region

        '** initialize

        Public Sub New()
            MyBase.New(constTableID)
        End Sub

        ''' <summary>
        ''' infuses the data object by record
        ''' </summary>
        ''' <param name="record"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub OnInfused(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnInfused

            Dim aType As otDataType
            Dim aValue As Object


            '***
            Try
                aValue = e.Record.GetValue(ConstFNordinals)

                If IsNumeric(aValue) Then
                    _ordinal = New Ordinal(CLng(Record.GetValue(ConstFNordinall)))
                Else
                    _ordinal = New Ordinal(CStr(Record.GetValue(ConstFNordinall)))
                End If

                ' get the keys and values
                Dim idstr As String = e.Record.GetValue(ConstFNIDs)
                Dim ids As String()
                If idstr <> "" AndAlso Not IsNull(idstr) Then
                    ids = SplitMultbyChar(idstr, ConstDelimiter)
                Else
                    ids = {}
                End If
                Dim valuestr As String = e.Record.GetValue(ConstFNValues)
                Dim values As String()
                If valuestr <> "" AndAlso Not IsNull(valuestr) Then
                    values = SplitMultbyChar(valuestr, ConstDelimiter)
                Else
                    values = {}
                End If
                Dim typestr As String = e.Record.GetValue(ConstFNTypes)
                Dim types As String()
                If typestr <> "" AndAlso Not IsNull(typestr) Then
                    types = SplitMultbyChar(typestr, ConstDelimiter)
                Else
                    types = {}
                End If

                For i = 0 To ids.Length - 1
                    If i < types.Length Then
                        Try
                            Select Case CLng(types(i))
                                Case CLng(otDataType.Bool)
                                    aType = otDataType.Bool
                                    aValue = CBool(values(i))
                                Case CLng(otDataType.[Date]), CLng(otDataType.[Timestamp]), CLng(otDataType.Time)
                                    aType = otDataType.[Date]
                                    aValue = CDate(values(i))
                                Case CLng(otDataType.Text)
                                    aType = otDataType.Text
                                    aValue = values(i)
                                Case CLng(otDataType.[Long])
                                    aType = otDataType.[Long]
                                    aValue = CLng(values(i))
                                Case Else
                                    Call CoreMessageHandler(subname:="XOutlineItem.infuse", messagetype:=otCoreMessageType.InternalError,
                                                            message:="Outline datatypes couldnot be determined ", arg1:=types(i))
                                    e.AbortOperation = True
                                    Exit Sub
                            End Select

                        Catch ex As Exception
                            Call CoreMessageHandler(exception:=ex, subname:="XOutlineItem.infuse",
                                                    messagetype:=otCoreMessageType.InternalError, message:="Outline keys couldnot be filled ")
                            e.AbortOperation = True
                            Exit Sub
                        End Try

                        '**
                        _keys.Add(New OTLineKey(aType, ids(i), aValue))
                    End If
                Next
                e.Proceed = True
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="XOutlineItem.Infuse")
                Unload()
                e.AbortOperation = True
            End Try

        End Sub
        ''' <summary>
        ''' retrieves a sorted list of items by uid
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function AllByID(ByVal id As String) As SortedList(Of Ordinal, XOutlineItem)
            Dim aCollection As New SortedList(Of Ordinal, XOutlineItem)
            Dim aRecordCollection As New List(Of ormRecord)
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim anEntry As New XOutlineItem


            Try
                aTable = ot.GetTableStore(constTableID)
                Dim aCommand As ormSqlSelectCommand = aTable.CreateSqlSelectCommand(id:="AllByID")
                If Not aCommand.Prepared Then
                    aCommand.OrderBy = "[" & constTableID & "." & ConstFNordinall & "] asc"
                    aCommand.Where = "[" & constFNID & "] = @ID"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@ID", ColumnName:=constFNID, tablename:=constTableID))
                    aCommand.Prepare()
                End If

                aCommand.SetParameterValue(ID:="@ID", value:=id)
                aRecordCollection = aCommand.RunSelect

                If aRecordCollection.Count > 0 Then
                    ' records read
                    For Each aRecord In aRecordCollection
                        ' add the Entry as Component
                        anEntry = New XOutlineItem
                        If InfuseDataObject(record:=aRecord, dataobject:=anEntry) Then
                            aCollection.Add(value:=anEntry, key:=anEntry.ordinal)
                        End If
                    Next aRecord

                End If
                Return aCollection
            Catch ex As Exception
                Call CoreMessageHandler(subname:="XOutlineItem.allByID", arg1:=id,
                                        exception:=ex, objectname:=constTableID)
                Return aCollection
            End Try


        End Function

        ''' <summary>
        ''' retrieves the data object from the data store
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="ordinal"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(ByVal id As String, ByVal ordinal As String) As Boolean
            Return Inject(id, New Ordinal(ordinal))
        End Function
        Public Overloads Function Inject(ByVal id As String, ByVal ordinal As Long) As Boolean
            Return Inject(id, New Ordinal(ordinal))
        End Function
        Public Overloads Function Inject(ByVal id As String, ByVal ordinal As Ordinal) As Boolean
            Dim pkarry() As Object = {id, ordinal.ToString}
            Return MyBase.Inject(pkarry)
        End Function
        ''' <summary>
        ''' create schema for persistency
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean

            Return ormDataObject.CreateDataObjectSchema(Of XOutlineItem)()


            '            ''''''''''''''''''''''''''''
            '            ''' THIS IS ONLY FOR LEGACY
            '            ''' 
            '            Dim UsedColumnNames As New Collection
            '            Dim aFieldDesc As New ormFieldDescription
            '            Dim PrimaryColumnNames As New Collection
            '            Dim LongOutlineColumnNames As New Collection
            '            Dim aTable As New ObjectDefinition
            '            Dim aTableEntry As New IObjectEntryDefinition


            '            aFieldDesc.ID = ""
            '            aFieldDesc.Parameter = ""
            '            aFieldDesc.Relation = New String() {}
            '            aFieldDesc.Aliases = New String() {}
            '            aFieldDesc.objectname = constTableID


            '            aTable = New ObjectDefinition
            '            aTable.Create(constTableID)

            '            '******
            '            '****** Fields

            '            With aTable


            '                On Error GoTo error_handle


            '                '*** TaskUID
            '                '****
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "outline id"
            '                aFieldDesc.ID = ""
            '                aFieldDesc.Parameter = ""
            '                aFieldDesc.ColumnName = "id"
            '                aFieldDesc.ID = "otl1"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
            '                LongOutlineColumnNames.Add(aFieldDesc.ColumnName)

            '                'Position
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "ordinal long"
            '                aFieldDesc.ColumnName = "ordinall"
            '                aFieldDesc.ID = "otli2"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                LongOutlineColumnNames.Add(aFieldDesc.ColumnName)

            '                'Position
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "ordinal string"
            '                aFieldDesc.ColumnName = "ordinals"
            '                aFieldDesc.ID = "otli3"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

            '                'uid
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "deliverable uid"
            '                aFieldDesc.ColumnName = "uid"
            '                aFieldDesc.ID = "dlvuid"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                UsedColumnNames.Add(aFieldDesc.ColumnName)
            '                UsedColumnNames.Add("id")
            '                UsedColumnNames.Add("ordinals")

            '                ' level
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "identlevel"
            '                aFieldDesc.ColumnName = "level"
            '                aFieldDesc.ID = "otli4"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' typeid
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "types of the outline key"
            '                aFieldDesc.ColumnName = "types"
            '                aFieldDesc.ID = "otli10"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' id
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "ids of the outline key"
            '                aFieldDesc.ColumnName = "ids"
            '                aFieldDesc.ID = "otli11"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' value #1
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "values of the outline key"
            '                aFieldDesc.ColumnName = "values"
            '                aFieldDesc.ID = "otli12"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.ID = ""
            '                aFieldDesc.Parameter = ""
            '                aFieldDesc.Relation = New String() {}
            '                aFieldDesc.Aliases = New String() {}

            '                '***
            '                '*** TIMESTAMP
            '                '****
            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "last Update"
            '                aFieldDesc.ColumnName = ConstFNUpdatedOn
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "creation Date"
            '                aFieldDesc.ColumnName = ConstFNCreatedOn
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' Index
            '                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
            '                Call .AddIndex("longOutline", LongOutlineColumnNames, isprimarykey:=False)
            '                Call .AddIndex("UsedOutline", UsedColumnNames, isprimarykey:=False)
            '                ' persist
            '                .Persist()
            '                ' change the database
            '                .CreateObjectSchema()
            '            End With

            '            CreateSchema = True
            '            Exit Function

            '            ' Handle the error
            'error_handle:
            '            Call CoreMessageHandler(subname:="XOutlineItem.createSchema", objectname:=constTableID)
            '            CreateSchema = False
        End Function
        ''' <summary>
        ''' Persist the data object to the datastore
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Overloads Function Persist(Optional timestamp As Date = constNullDate) As Boolean

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Persist = False
                    Exit Function
                End If
            End If

            Try
                Call Me.Record.SetValue(constFNID, _id)
                '** own feed record
                If _ordinal.Type = OrdinalType.longType Then
                    Call Me.Record.SetValue(ConstFNordinall, _ordinal.Value)
                Else
                    Call Me.Record.SetValue(ConstFNordinall, 0)
                End If

                Call Me.Record.SetValue(ConstFNordinals, _ordinal.ToString)
                Call Me.Record.SetValue(ConstFNIdent, _level)

                '***
                Dim idstr As String = ConstDelimiter
                Dim valuestr As String = ConstDelimiter
                Dim typestr As String = ConstDelimiter

                For Each key As OTLineKey In _keys
                    idstr &= key.ID & ConstDelimiter
                    If key.ID.ToLower = "uid" Then
                        Me.Record.SetValue(ConstFNUid, CLng(key.Value))
                    End If
                    typestr &= CLng(key.Type) & ConstDelimiter
                    valuestr &= CStr(key.Value) & ConstDelimiter
                Next

                If idstr = ConstDelimiter Then idstr = ""
                If valuestr = ConstDelimiter Then valuestr = ""
                If typestr = ConstDelimiter Then typestr = ""

                Call Me.Record.SetValue(ConstFNIDs, UCase(idstr))
                Call Me.Record.SetValue(ConstFNValues, valuestr)
                Call Me.Record.SetValue(ConstFNTypes, LCase(typestr))

                'Call me.record.setValue(OTDBConst_UpdateOn, (Date & " " & Time)) not necessary
                Return MyBase.Persist(timestamp:=timestamp, doFeedRecord:=True)

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="XOutlineItem.persist")
                Return False
            End Try


        End Function

        ''' <summary>
        ''' create a new outline item in the persistable data store
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <param name="ordinal"></param>
        ''' <param name="uid"></param>
        ''' <param name="level"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal ID As String, ByVal ordinal As String, Optional uid As Long = 0, Optional level As UShort = 0) As Boolean
            Return Create(ID, New Ordinal(ordinal), uid, level)
        End Function
        Public Overloads Function Create(ByVal ID As String, ByVal ordinal As Long, Optional uid As Long = 0, Optional level As UShort = 0) As Boolean
            Return Create(ID, New Ordinal(ordinal), uid, level)
        End Function
        Public Overloads Function Create(ByVal ID As String, ByVal ordinal As Ordinal, Optional uid As Long = 0, Optional level As UShort = 0) As Boolean
            Dim pkarry() As Object = {ID, ordinal.ToString}

            If MyBase.Create(pkarry, checkUnique:=True) Then
                ' set the primaryKey
                _id = ID
                _ordinal = ordinal
                _keys = New List(Of OTLineKey)
                If uid <> 0 Then
                    _keys.Add(New OTLineKey(otDataType.Long, "uid", uid))
                End If

                _level = level
                Return Me.IsCreated
            End If

            Return False
        End Function
    End Class
End Namespace
