Option Explicit On
Imports System.Diagnostics.Debug
Imports System.Collections.Specialized

Imports OnTrack
Imports OnTrack.Database
Imports OnTrack.Scheduling
Imports OnTrack.Deliverables
Imports OnTrack.Parts
Imports OnTrack.Configurables
Imports OnTrack.XChange.ConvertRequestEventArgs


REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** XChangeManager Classes for On Track Database Backend Library
REM ***********
REM *********** Version: X.YY
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************
Namespace OnTrack.XChange
    ''' <summary>
    ''' Arguments for the ConvertRequest and Result Arguments
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ConvertRequestEventArgs
        Inherits EventArgs

        Public Enum convertValueType
            Hostvalue
            DBValue
        End Enum

        Private _valuetype As convertvaluetype
        Private _hostvalue As Object = Nothing
        Private _dbvalue As Object = Nothing
        Private _HostValueisNull As Boolean = False
        Private _HostValueisEmpty As Boolean = False
        Private _dbValueisNull As Boolean = False
        Private _dbValueIsEmpty As Boolean = False
        Private _datatype As otFieldDataType = 0

        ' result
        Private _convertSucceeded As Boolean = False
        Private _msglog As ObjectLog

        Public Sub New(datatype As otFieldDataType, valuetype As convertValueType, value As Object,
                       Optional isnull As Boolean = False, Optional isempty As Boolean = False, Optional msglog As ObjectLog = Nothing)
            _datatype = datatype
            _valuetype = valuetype
            Me.Value = value
            Me.IsEmpty = isempty
            Me.IsNull = isnull

            _msglog = msglog
        End Sub

#Region "Properties"

        ''' <summary>
        ''' Gets or sets the is null.
        ''' </summary>
        ''' <value>The is null.</value>
        Public Property IsNull() As Boolean
            Get
                If _valuetype = convertValueType.Hostvalue Then
                    Return Me._HostValueisNull
                Else
                    Return Me._dbValueisNull
                End If
            End Get
            Set(value As Boolean)
                Me._HostValueisNull = value
                If _valuetype = convertValueType.Hostvalue Then
                    Me._HostValueisNull = value
                Else
                    Me._dbValueisNull = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the is empty.
        ''' </summary>
        ''' <value>The is empty.</value>
        Public Property IsEmpty() As Boolean
            Get
                If _valuetype = convertValueType.Hostvalue Then
                    Return Me._HostValueisEmpty
                Else
                    Return Me._dbValueIsEmpty
                End If
            End Get
            Set(value As Boolean)
                Me._HostValueisNull = value
                If _valuetype = convertValueType.Hostvalue Then
                    Me._HostValueisEmpty = value
                Else
                    Me._dbValueIsEmpty = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the datatype.
        ''' </summary>
        ''' <value>The datatype.</value>
        Public Property Datatype() As otFieldDataType
            Get
                Return Me._datatype
            End Get
            Set(value As otFieldDataType)
                Me._datatype = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the msglog.
        ''' </summary>
        ''' <value>The msglog.</value>
        Public Property Msglog() As ObjectLog
            Get
                Return Me._msglog
            End Get
            Set(value As ObjectLog)
                Me._msglog = value
            End Set
        End Property


        ''' <summary>
        ''' Gets or sets the convert succeeded.
        ''' </summary>
        ''' <value>The convert succeeded.</value>
        Public Property ConvertSucceeded() As Boolean
            Get
                Return Me._convertSucceeded
            End Get
            Set(value As Boolean)
                Me._convertSucceeded = value
            End Set
        End Property
        ''' <summary>
        ''' returns the value to be converted
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Value As Object
            Get
                If _valuetype = convertValueType.DBValue Then
                    Return _dbvalue
                Else
                    Return _hostvalue
                End If
            End Get
            Set(value As Object)
                If _valuetype = convertValueType.DBValue Then
                    _dbvalue = value
                    _hostvalue = Nothing
                Else
                    _dbvalue = Nothing
                    _hostvalue = value
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns the converted value 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ConvertedValue As Object
            Get
                If _valuetype = convertValueType.Hostvalue Then
                    Return _dbvalue
                Else
                    Return _hostvalue
                End If
            End Get
            Set(value As Object)
                If _valuetype = convertValueType.Hostvalue Then
                    _dbvalue = value
                    _hostvalue = Nothing
                Else
                    _dbvalue = Nothing
                    _hostvalue = value
                End If
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the dbvalue.
        ''' </summary>
        ''' <value>The dbvalue.</value>
        Public Property Dbvalue() As Object
            Get
                Return Me._dbvalue
            End Get
            Set(value As Object)
                Me._dbvalue = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the hostvalue.
        ''' </summary>
        ''' <value>The hostvalue.</value>
        Public Property Hostvalue() As Object
            Get
                Return Me._hostvalue
            End Get
            Set(value As Object)
                Me._hostvalue = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the host valueis null.
        ''' </summary>
        ''' <value>The host valueis null.</value>
        Public Property HostValueisNull() As Boolean
            Get
                Return Me._HostValueisNull
            End Get
            Set(value As Boolean)
                Me._HostValueisNull = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the host valueis empty.
        ''' </summary>
        ''' <value>The host valueis empty.</value>
        Public Property HostValueisEmpty() As Boolean
            Get
                Return Me._HostValueisEmpty
            End Get
            Set(value As Boolean)
                Me._HostValueisEmpty = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the db valueis null.
        ''' </summary>
        ''' <value>The db valueis null.</value>
        Public Property DbValueisNull() As Boolean
            Get
                Return Me._dbValueisNull
            End Get
            Set(value As Boolean)
                Me._dbValueisNull = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the db value is empty.
        ''' </summary>
        ''' <value>The db value is empty.</value>
        Public Property DbValueIsEmpty() As Boolean
            Get
                Return Me._dbValueIsEmpty
            End Get
            Set(value As Boolean)
                Me._dbValueIsEmpty = value
            End Set
        End Property

#End Region
    End Class

    ''' <summary>
    ''' Interface for XConfigMembers
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface iConfigMember
        Inherits iormPersistable
        Inherits iormInfusable

        ''' <summary>
        ''' returns the entryname
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Property Entryname() As String

        ''' <summary>
        ''' returns the ID of the ConfigMember
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ID() As String
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
        ''' gets or sets parameter
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Parameter() As String

        ''' <summary>
        ''' gets or sets relation
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Relation() As Object

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
        ReadOnly Property IsAttributeEntry() As Boolean

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
        ReadOnly Property IsField() As Boolean

        ''' <summary>
        ''' gets True if this is an Object Entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsObjectEntry() As Boolean

        ''' <summary>
        ''' gets or sets the ordinal
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ordinal() As Ordinal

        ''' <summary>
        ''' gets or sets the OrderedBy Flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property IsOrderedBy() As Boolean



    End Interface

    ''' <summary>
    ''' describes an XChange XConfigMember ObjectEntry (Object is usually the Table)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class XConfigObjectEntry
        Inherits XConfigMember
        Implements iConfigMember

#Region "Properties"

        ''' <summary>
        ''' gets or sets the Attribute Entry Flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property IsAttributeEntry() As Boolean Implements iConfigMember.IsAttributeEntry
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
        Public Overrides ReadOnly Property IsField() As Boolean Implements iConfigMember.IsField
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
        Public Overrides ReadOnly Property IsObjectEntry() As Boolean Implements iConfigMember.IsObjectEntry
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
            MyBase._isAttributeEntry = False
            MyBase._isObjectEntry = True
        End Sub
    End Class
    ''' <summary>
    ''' describes the XConfig Member Attribute
    ''' </summary>
    ''' <remarks></remarks>
    Public Class XConfigAttributeEntry
        Inherits XConfigMember
        Implements iConfigMember

#Region "Properties"

        ''' <summary>
        ''' gets or sets the XChange ID for the Member
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Property ID() As String Implements iConfigMember.ID
            Get
                ID = _xid
            End Get
            Set(avalue As String)

                If LCase(_xid) <> LCase(avalue) Then
                    _xid = avalue
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets the fieldname
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Property Entryname() As String Implements iConfigMember.Entryname
            Get
                Entryname = _entryname
            End Get
            Set(avalue As String)
                If LCase(_entryname) <> LCase(avalue) Then
                    _entryname = avalue
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' gets the Aliases of the XChange Member
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Overloads ReadOnly Property Aliases() As List(Of String) Implements iConfigMember.Aliases
            Get
                If Not Me.ObjectEntryDefinition Is Nothing Then
                    Aliases = _EntryDefinition.Aliases.ToList
                Else
                    Aliases = New List(Of String)
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
        Overloads ReadOnly Property HasAlias([alias] As String) As Boolean Implements iConfigMember.HasAlias
            Get
                If Not Me.ObjectEntryDefinition Is Nothing Then
                    _aliases = _EntryDefinition.Aliases
                Else
                    Return False
                End If
                Return _aliases.Contains(UCase([alias]))

            End Get
        End Property

        ''' <summary>
        ''' gets or sets the Attribute Entry Flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property IsAttributeEntry() As Boolean Implements iConfigMember.IsAttributeEntry
            Get
                Return True
            End Get

        End Property

        ''' <summary>
        ''' gets True if this is a Compound
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads ReadOnly Property IsCompound() As Boolean Implements iConfigMember.IsCompound
            Get
                Dim aFieldDef As ObjectEntryDefinition
                If _isAttributeEntry Then
                    aFieldDef = Me.ObjectEntryDefinition
                    If Not aFieldDef Is Nothing Then
                        IsCompound = aFieldDef.IsCompound
                        Exit Property
                    End If
                End If
                IsCompound = False
            End Get

        End Property


        ''' <summary>
        ''' gets True if the Attribute is a Field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads ReadOnly Property IsField() As Boolean Implements iConfigMember.IsField
            Get
                Dim aFieldDef As ObjectEntryDefinition
                If Me.IsAttributeEntry Then
                    aFieldDef = Me.[ObjectEntryDefinition]
                    If Not aFieldDef Is Nothing Then
                        IsField = aFieldDef.IsField
                        Exit Property
                    End If
                End If
                IsField = False
            End Get
        End Property

        ''' <summary>
        ''' gets True if this is an Object Entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property IsObjectEntry() As Boolean Implements iConfigMember.IsObjectEntry
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
                IsDynamicAttribute = _isDynamicAttribute And _isAttributeEntry
            End Get
            Set(value As Boolean)
                If _isDynamicAttribute <> value And _isAttributeEntry Then
                    _isDynamicAttribute = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
#End Region

        ''' <summary>
        ''' sets the XChange Member to the values of a FieldDescription
        ''' </summary>
        ''' <param name="aFieldDesc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetByFieldDesc(fielddesc As ormFieldDescription) As Boolean
            If Not Me.IsLoaded And Not Me.IsCreated Then
                Return False
            End If

            If fielddesc.ID <> "" Then
                _xid = fielddesc.ID
                _aliases = fielddesc.Aliases
            Else
                _entryname = fielddesc.ColumnName
            End If
            Me.Objectname = fielddesc.Tablename

            Return Me.IsChanged
        End Function

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase._isAttributeEntry = True
            MyBase._isObjectEntry = False
        End Sub
    End Class

    ''' <summary>
    ''' describes a Xconfig Member - an individual item
    ''' </summary>
    ''' <remarks></remarks>

    Public MustInherit Class XConfigMember
        Inherits ormDataObject
        Implements iormInfusable, iormPersistable, iConfigMember

        '***
        '*** Meta Definition PErsistency
        <ormSchemaTableAttribute(version:=3)> Public Const ConstTableID = "tblXChangeConfigMembers"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=50, primaryKeyordinal:=1,
                        title:="XChangeConfigID", description:="name of the XchangeConfiguration")>
        Public Const ConstFNID = "configname"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Long, primaryKeyordinal:=2,
                        title:="IndexNo", description:="position in the the XchangeConfiguration")>
        Public Const ConstFNIDNo = "idno"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=100,
                        title:="ObjectName", description:="Name of the ObjectDefinition")>
        Public Const constFNObjectname = "objectname"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=100,
                        title:="EntryName", description:="Name of the Entry in theObjectDefinition")>
        Public Const constFNEntryname = "entryname"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=255,
                        title:="Description", description:="Description of the Entry")>
        Public Const constFNDesc = "desc"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Memo, title:="Comment", description:="Comment")>
        Public Const constFNComment = "cmt"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=50,
                        title:="XChange ID", description:="ID  of the Attribute in theObjectDefinition")>
        Public Const constFNXID = "id"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=250,
                        title:="Parameter", description:="Parameter for the Attribute")>
        Public Const constFNParameter = "parameter"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=250,
                        title:="ordinal", description:="ordinal for the Attribute Mapping")>
        Public Const constFNordinal = "ordinal"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=250,
                        title:="Relation", description:="Relation for the Attribute")>
        Public Const constFNRelation = "relation"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, title:="Is Object Entry", description:="Set if this is an object entry")>
        Public Const constFNIsObjectEntry = "isobj"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, title:="Is Attribute Entry", description:="Set if this is an compound entry")>
        Public Const constFNIsAttributeEntry = "isattr"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, title:="Is Compound Entry", description:="Set if this is an compound entry")>
        Public Const constFNIsCompoundEntry = "iscomp"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, title:="Is Entry Read-Only", description:="Set if this entry is read-only - value in OTDB cannot be overwritten")>
        Public Const constFNIsReadonly = "isro"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, title:="Is ordered", description:="Set if this entry is ordered")>
        Public Const constFNIsOrder = "isorder"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, title:="Is dynamic attribute", description:="Set if this entry is dynamic")>
        Public Const constFNIsDynamic = "isdynamic"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, title:="Attribute is not exchanged", description:="Set if this attribute is not exchanged")>
        Public Const constFNIsNotXChanged = "isnxchg"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.List, size:=50, parameter:="parameter_xcmd_list",
                        title:="XChange Command", description:="XChangeCommand to run on this")>
        Public Const constFNXCMD = "xcmd"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Long, title:="Order Number", description:="Order number in which entriy is processed")>
        Public Const constFNOrderNo = "orderno"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=250, title:="MessageLogTag", description:="Message Log Tag")>
        Public Const constFNMsgLogTag = "msglogtag"

        ' fields
        <ormColumnMappingAttribute(fieldname:=ConstFNID)> Protected _configname As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNIDNo)> Protected _idno As Long
        <ormColumnMappingAttribute(fieldname:=constFNXID)> Protected _xid As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNObjectname)> Protected _objectname As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNEntryname)> Protected _entryname As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNRelation)> Protected _relation As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNParameter)> Protected _parameter As String = ""
        '<otColumnMapping(fieldname:=constFNordinal)> do not since we cannot map it
        Private _ordinal As Ordinal = New Ordinal(0)
        <ormColumnMappingAttribute(fieldname:=constFNComment)> Protected _cmt As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNDesc)> Protected _desc As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNIsNotXChanged)> Protected _isNotXChanged As Boolean
        <ormColumnMappingAttribute(fieldname:=constFNIsReadonly)> Protected _isReadOnly As Boolean
        <ormColumnMappingAttribute(fieldname:=constFNIsAttributeEntry)> Protected _isAttributeEntry As Boolean
        <ormColumnMappingAttribute(fieldname:=constFNIsObjectEntry)> Protected _isObjectEntry As Boolean
        <ormColumnMappingAttribute(fieldname:=constFNIsCompoundEntry)> Protected _isCompundEntry As Boolean
        <ormColumnMappingAttribute(fieldname:=constFNXCMD)> Protected _xcmd As otXChangeCommandType = 0
        <ormColumnMappingAttribute(fieldname:=constFNIsOrder)> Protected _isOrdered As Boolean
        <ormColumnMappingAttribute(fieldname:=constFNOrderNo)> Protected _orderNo As Long
        <ormColumnMappingAttribute(fieldname:=constFNIsDynamic)> Protected _isDynamicAttribute As Boolean

        'dynamic
        Protected _EntryDefinition As ObjectEntryDefinition
        Protected _ObjectDefinition As ObjectDefinition
        Protected _aliases As String()    ' not saved !
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
        Public Property ID() As String Implements iConfigMember.ID
            Get
                ID = _xid
            End Get
            Set(avalue As String)

                If LCase(_xid) <> LCase(avalue) Then
                    _xid = avalue
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' gets the fieldname
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Entryname() As String Implements iConfigMember.Entryname
            Get
                Entryname = _entryname
            End Get
            Set(avalue As String)
                If LCase(_entryname) <> LCase(avalue) Then
                    _entryname = avalue
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the objectname
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Objectname() As String Implements iConfigMember.Objectname
            Get
                Objectname = _objectname
            End Get
            Set(value As String)
                If LCase(_objectname) <> LCase(value) Then
                    _objectname = LCase(value)
                    Me.IsChanged = True
                End If
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
        Public ReadOnly Property Msglogtag() As String Implements iConfigMember.Msglogtag
            Get
                If _msglogtag = "" Then
                    _msglogtag = GetUniqueTag()
                End If
                Msglogtag = _msglogtag
            End Get

        End Property

        ''' <summary>
        ''' gets the configname
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Configname() As String Implements iConfigMember.Configname
            Get
                Configname = _configname
            End Get
            Set(value As String)
                If LCase(_configname) <> LCase(value) Then
                    _configname = value
                    Me.IsChanged = True
                End If
            End Set
        End Property


        ''' <summary>
        ''' gets the Aliases of the XChange Member
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Aliases() As List(Of String) Implements iConfigMember.Aliases
            Get
                If Not Me.ObjectEntryDefinition Is Nothing Then
                    Aliases = _EntryDefinition.Aliases.ToList
                Else
                    Aliases = New List(Of String)
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
        ReadOnly Property HasAlias([alias] As String) As Boolean Implements iConfigMember.HasAlias
            Get
                If Not Me.ObjectEntryDefinition Is Nothing Then
                    _aliases = _EntryDefinition.Aliases
                Else
                    Return False
                End If
                Return _aliases.Contains(UCase([alias]))

            End Get
        End Property

        ''' <summary>
        ''' gets or sets the Xchange Command
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property XChangeCmd() As otXChangeCommandType Implements iConfigMember.XChangeCmd
            Get
                XChangeCmd = _xcmd
            End Get
            Set(value As otXChangeCommandType)
                If _xcmd <> value Then
                    _xcmd = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets the ObjectEntry Definition for the XChange Member
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property [ObjectEntryDefinition] As ObjectEntryDefinition
            Get
                Dim anEntryDefinition As ObjectEntryDefinition
                If (Me.IsCreated Or Me.IsLoaded) And _isAttributeEntry And _EntryDefinition Is Nothing Then

                    If _entryname <> "" And Me.Objectname <> "" Then
                        anEntryDefinition = CurrentSession.Objects.GetEntry(objectname:=Me.Objectname, entryname:=_entryname)
                    ElseIf Me.Objectname <> "" And _xid <> "" Then
                        anEntryDefinition = CurrentSession.Objects.GetEntryByID(id:=_xid, objectname:=Me.Objectname).First
                    Else
                        anEntryDefinition = CurrentSession.Objects.GetEntryByID(id:=_xid).First
                    End If
                    If Not anEntryDefinition Is Nothing Then
                        _EntryDefinition = anEntryDefinition
                    End If
                End If

                Return _EntryDefinition
                ' return
                [ObjectEntryDefinition] = Nothing
            End Get
            Set(value As ObjectEntryDefinition)
                _EntryDefinition = value
            End Set
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
        Public Property ordinal() As Ordinal Implements iConfigMember.ordinal
            Get
                ordinal = _ordinal
            End Get
            Set(value As Ordinal)
                _ordinal = value
                Me.IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' gets or sets parameter
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Parameter() As String Implements iConfigMember.Parameter
            Get
                Parameter = _parameter
            End Get
            Set(value As String)
                If LCase(_parameter) <> LCase(value) Then
                    _parameter = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets relation
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Relation() As Object Implements iConfigMember.Relation
            Get
                Relation = SplitMultiDelims(text:=_relation, DelimChars:=ConstDelimiter)
            End Get
            Set(avalue As Object)
                Dim i As Integer
                If IsArrayInitialized(avalue) Then
                    Dim aStrValue As String = ""
                    For i = LBound(avalue) To UBound(avalue)
                        If i = LBound(avalue) Then
                            aStrValue = avalue(i)
                        Else
                            aStrValue = aStrValue & ConstDelimiter & avalue(i)
                        End If
                    Next i
                    _relation = aStrValue
                    Me.IsChanged = True
                ElseIf Not IsEmpty(Trim(avalue)) And Trim(avalue) <> "" And Not IsNull(avalue) Then
                    _relation = CStr(Trim(avalue))
                Else
                    _relation = ""
                End If
            End Set
        End Property

        ''' <summary>
        ''' gets or sets comment
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Comment() As String
            Get
                Comment = _cmt
            End Get
            Set(value As String)
                If _cmt <> value Then
                    _cmt = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' Primary Key Indexno
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Indexno() As Long Implements iConfigMember.Indexno
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
        Public Property IsXChanged() As Boolean Implements iConfigMember.IsXChanged
            Get
                IsXChanged = Not _isNotXChanged
            End Get
            Set(value As Boolean)
                If _isNotXChanged <> Not value Then
                    _isNotXChanged = Not value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' sets the Readonly Flag - value of the OTDB cannot be overwritten
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsReadOnly() As Boolean Implements iConfigMember.IsReadOnly
            Get
                IsReadOnly = _isReadOnly
            End Get
            Set(value As Boolean)
                If _isReadOnly <> value Then
                    _isReadOnly = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the Attribute Entry Flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property IsAttributeEntry() As Boolean Implements iConfigMember.IsAttributeEntry

        ''' <summary>
        ''' gets True if this is a Compound
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property IsCompound() As Boolean Implements iConfigMember.IsCompound
            Get
                Dim aFieldDef As ObjectEntryDefinition
                If _isAttributeEntry Then
                    aFieldDef = Me.ObjectEntryDefinition
                    If Not aFieldDef Is Nothing Then
                        IsCompound = aFieldDef.IsCompound
                        Exit Property
                    End If
                End If
                IsCompound = False
            End Get

        End Property
        ''' <summary>
        ''' gets True if the Attribute is a Field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property IsField() As Boolean Implements iConfigMember.IsField
            Get
                Dim aFieldDef As ObjectEntryDefinition
                If Me.IsAttributeEntry Then
                    aFieldDef = Me.[ObjectEntryDefinition]
                    If Not aFieldDef Is Nothing Then
                        IsField = aFieldDef.IsField
                        Exit Property
                    End If
                End If
                IsField = False
            End Get
        End Property

        ''' <summary>
        ''' gets True if this is an Object Entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property IsObjectEntry() As Boolean Implements iConfigMember.IsObjectEntry

        ''' <summary>
        ''' gets or sets the OrderedBy Flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsOrderedBy() As Boolean Implements iConfigMember.IsOrderedBy
            Get
                IsOrderedBy = _isOrdered
            End Get
            Set(value As Boolean)
                If _isOrdered <> value Then
                    _isOrdered = value
                    Me.IsChanged = True
                End If
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
                Orderno = _orderNo
            End Get
            Set(value As Long)
                _orderNo = value
                Me.IsChanged = True
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
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean Implements iormInfusable.Infuse
            Dim aValue As Object

            Try
                If MyBase.Infuse(record) Then
                    If IsNull(record.GetValue(constFNordinal)) Then
                        _ordinal = New Ordinal(0)
                    Else
                        aValue = record.GetValue(constFNordinal)
                        If IsNumeric(aValue) Then
                            _ordinal = New Ordinal(CLng(aValue))
                        Else
                            _ordinal = New Ordinal(CStr(aValue))
                        End If
                    End If
                End If

                Return Me.IsLoaded
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="XConfigMember.Infuse")
                Unload()
                Return False
            End Try

        End Function

        ''' <summary>
        ''' Load XChange Member from persistence store
        ''' </summary>
        ''' <param name="configname"></param>
        ''' <param name="indexno"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal configname As String, ByVal indexno As Long) As Boolean
            Dim pkarry() As Object = {LCase(configname), indexno}
            Return MyBase.LoadBy(pkarry)
        End Function
        ''' <summary>
        ''' Create Persistence Schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateSchema(Of clsOTDBXChangeMember)()
        End Function
        ''' <summary>
        ''' Persist the Xchange Member
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean
            Try
                '* ordinal
                If Me.ordinal = New Ordinal(0) And Orderno <> 0 Then
                    Me.ordinal = New Ordinal(Orderno)
                End If
                If Orderno = 0 And Me.ordinal <> New Ordinal(0) And Me.ordinal.Type = ordinalType.longType Then
                    Me.Orderno = Me.ordinal.Value
                End If
                Call Me.Record.SetValue(constFNordinal, _ordinal.Value.ToString)
                Return MyBase.Persist(timestamp, doFeedRecord:=True)
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="XConfigMember.Persist")
                Return False
            End Try
        End Function
        ''' <summary>
        ''' creates a persistable XChange member with primary Key
        ''' </summary>
        ''' <param name="configname"></param>
        ''' <param name="indexno"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal configname As String, Optional ByVal indexno As Long = 0) As Boolean
            Dim pkarray() As Object = {LCase(configname), indexno}
            If MyBase.Create(pkArray:=pkarray, checkUnique:=False) Then
                ' set the primaryKey
                _configname = LCase(configname)
                _idno = indexno
                Return Me.IsCreated
            Else
                Return False
            End If
        End Function

    End Class


    '************************************************************************************
    '***** CLASS XConfig defines how data can be exchanged with the XChange Manager
    '*****      
    '*****

    Public Class XConfig
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        'Implements iOTDBXChange

        <ormSchemaTableAttribute(Version:=2)> Public Const constTableID = "tblXChangeConfigs"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=50, primaryKeyordinal:=1,
             Title:="Name", Description:="Name of XChange Configuration")>
        Public Const constFNID = "configname"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=255,
             Title:="Description", Description:="Description of XChange Configuration")>
        Public Const constFNDesc = "desc"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Memo,
             Title:="Comments", Description:="Comments")>
        Public Const constFNTitle = "cmt"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool,
             Title:="IsDynamic", Description:="the XChange Config accepts dynamic addition of XChangeIDs")>
        Public Const constFNDynamic = "isdynamic"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=50,
               Title:="Outline ID", Description:="ID to the associated Outline")>
        Public Const constFNOutline = "outline"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=255,
              Title:="Message Log Tag", Description:="Message Log Tag")>
        Public Const constFNMsgLogTag = "msglogtag"


        ' fields
        <ormColumnMappingAttribute(fieldname:=constFNID)> Private _configname As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNDesc)> Private _description As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNMsgLogTag)> Private _msglogtag As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNDynamic)> Private _DynamicAttributes As Boolean
        <ormColumnMappingAttribute(fieldname:=constFNOutline)> Private _outlineid As String = ""


        Private _msglog As New ObjectLog
        Private _processedDate As Date = ConstNullDate

        ' members itself per key:=indexnumber, item:=clsOTDBXChangeMember
        Private _members As New SortedDictionary(Of Long, iConfigMember)
        Private _membersByordinal As New SortedDictionary(Of Ordinal, List(Of iConfigMember))

        ' reference object order list to work through members in the row of the exchange
        Private _objectsDirectory As New Dictionary(Of String, XConfigObjectEntry)
        Private _objectsByOrderDirectory As New SortedDictionary(Of Long, XConfigObjectEntry)

        ' reference Attributes list to work
        Private _attributesIDDirectory As New Dictionary(Of String, XConfigAttributeEntry)
        Private _attributesByObjectnameDirectory As New Dictionary(Of String, List(Of XConfigAttributeEntry))
        Private _attributesIDList As New Dictionary(Of String, List(Of XConfigAttributeEntry)) ' list if IDs are not unique
        Private _aliasDirectory As New Dictionary(Of String, List(Of XConfigAttributeEntry))

        ' object ordinalMember -> Members which are driving the ordinal of the complete eXchange
        ' Private _orderByMembers As New Dictionary(Of Object, clsOTDBXChangeMember)

        '** dynamic outline
        Dim _outline As New XOutline

        '** initialize
        Public Sub New()
            Call MyBase.New(constTableID)
            'me.record.tablename = ourTableName
            _msglog = New ObjectLog

        End Sub

#Region "Properties"


        ''' <summary>
        ''' Gets or sets the S outlineid.
        ''' </summary>
        ''' <value>The S outlineid.</value>
        Public Property OutlineID() As String
            Get
                Return Me._outlineid
            End Get
            Set(value As String)
                Me._outlineid = value
                _outline = Nothing
            End Set
        End Property
        ReadOnly Property Outline As XOutline
            Get
                If Me._outlineid <> "" And (_IsLoaded Or Me.IsCreated) Then
                    If Not _outline.IsLoaded And Not _outline.IsCreated Then
                        If _outline.LoadBy(Me._outlineid) Then
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
                Me._DynamicAttributes = value
            End Set
        End Property

        '****** getUniqueTag
        Public Function GetUniqueTag()
            GetUniqueTag = ConstDelimiter & constTableID & ConstDelimiter & _configname & ConstDelimiter & "0" & ConstDelimiter
        End Function
        ReadOnly Property Msglogtag() As String
            Get
                If _msglogtag = "" Then
                    _msglogtag = GetUniqueTag()
                End If
                msglogtag = _msglogtag
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

        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                _description = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property ProcessedDate() As Date
            Get
                ProcessedDate = _processedDate
            End Get
            Set(value As Date)
                _processedDate = value
                Me.IsChanged = True
            End Set
        End Property

        ReadOnly Property NoAttributes() As Long
            Get
                NoAttributes = _attributesIDDirectory.Count
            End Get

        End Property

        ReadOnly Property NoObjects() As Long
            Get
                NoObjects = _objectsDirectory.Count
            End Get
        End Property

        ReadOnly Property NoMembers() As Long
            Get
                NoMembers = _members.Count - 1
            End Get
        End Property
#End Region


        ''' <summary>
        '''  get the maximal ordinal as long if it is numeric
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMaxordinalNo() As Long
            If Not IsCreated And Not IsLoaded Then
                GetMaxordinalNo = 0
                Exit Function
            End If

            Return _members.Keys.Max()
        End Function

        ''' <summary>
        ''' returns the maximal index number
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMaxIndexNo() As Long

            If NoMembers >= 0 Then
                Return Me.MemberIndexNo.Max
            Else
                Return 0
            End If

        End Function
        ''' <summary>
        ''' returns the max order number 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMaxObjectOrderNo() As Long
            Dim keys As List(Of Long)

            If NoMembers >= 0 Then
                keys = Me.ObjectOrderNumbers
                If keys.Count > 0 Then
                    Return keys.Max
                Else
                    Return 0
                End If
            Else
                Return 0
            End If

        End Function


        '*** get the highest need XCMD to run the attributes XCMD
        '***
        Public Function GetHighestXCmd() As otXChangeCommandType

            Dim aHighestXcmd As otXChangeCommandType

            aHighestXcmd = 0

            Dim listofObjects As List(Of XConfigObjectEntry) = Me.Objects
            If listofObjects.Count = 0 Then
                Return 0
            End If

            For Each aChangeMember As XConfigObjectEntry In listofObjects
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

            Dim listofAttributes As List(Of XConfigAttributeEntry) = Me.Attributes(objectname:=objectname)
            If listofAttributes.Count = 0 Then
                Return 0
            End If

            For Each aChangeMember As XConfigAttributeEntry In listofAttributes
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
        Public Function SetordinalForID(ByVal ID As String, ByVal ordinal As Object, Optional ByVal objectname As String = "") As Boolean
            Dim anEntry As New XConfigAttributeEntry()
            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                SetordinalForID = False
                Exit Function
            End If

            ' get the entry
            anEntry = Me.AttributeByID(ID, objectname)
            If anEntry Is Nothing Then
                Return False
            ElseIf Not anEntry.IsLoaded And Not anEntry.IsCreated Then
                Return False
            End If

            If Not TypeOf ordinal Is OnTrack.Ordinal Then
                ordinal = New Ordinal(ordinal)
            End If
            anEntry.ordinal = ordinal
            AddordinalReference(anEntry)
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
            Dim aMember As New XConfigObjectEntry

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                SetObjectXCmd = False
                Exit Function
            End If

            ' return if exists
            If Not _objectsDirectory.ContainsKey(key:=name) Then
                SetObjectXCmd = False
                Exit Function
            Else
                aMember = _objectsDirectory.Item(key:=name)
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

            Dim aMember As New XConfigObjectEntry
            Dim anObjectDef As ObjectDefinition = CurrentSession.Objects.GetObject(name)


            ' Nothing
            If Not Me.IsLoaded And Not Me.IsCreated Then
                Return False
            End If

            ' return if exists
            If _objectsDirectory.ContainsKey(key:=LCase(name)) Then
                If xcmd = 0 Then
                    aMember = _objectsDirectory.Item(key:=LCase(name))
                    xcmd = aMember.XChangeCmd
                End If
                Call SetObjectXCmd(name:=name, xchangecommand:=xcmd)
                Return False
            End If

            ' load
            If anObjectDef Is Nothing Then
                Return False
            End If

            ' add the component
            aMember = New XConfigObjectEntry
            If aMember.Create(Me.Configname, Me.GetMaxIndexNo + 1) Then
                aMember.ordinal.Value = New Ordinal(0)
                aMember.Objectname = name


                If orderno = 0 Then
                    aMember.Orderno = Me.GetMaxObjectOrderNo + 1
                Else
                    aMember.Orderno = orderno
                End If
                aMember.ordinal.Value = orderno
                If IsMissing(xcmd) Then
                    xcmd = otXChangeCommandType.Read
                End If
                aMember.XChangeCmd = xcmd

                Return Me.AddMember(aMember)
            End If

            Return False

        End Function

        ''' <summary>
        ''' Adds an atribute by fieldname
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <param name="ordinal"></param>
        ''' <param name="OBJECTNAME"></param>
        ''' <param name="ISXCHANGED"></param>
        ''' <param name="XCMD"></param>
        ''' <param name="READONLY"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddAttributeByField(ByRef entryname As String,
                                             ByVal objectname As String,
                                                Optional ByVal ordinal As Object = Nothing,
                                                Optional ByVal isXChanged As Boolean = True,
                                                Optional ByVal xcmd As otXChangeCommandType = Nothing,
                                                Optional ByVal [readonly] As Boolean = False) As Boolean

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then

                AddAttributeByField = False
                Exit Function
            End If

            Dim anFieldEntry As ObjectEntryDefinition = CurrentSession.Objects.GetEntry(objectname:=objectname, entryname:=entryname)


            If Not anFieldEntry Is Nothing Then
                Return Me.AddAttributeByField(objectentry:=anFieldEntry, objectname:=objectname, ordinal:=ordinal, isxchanged:=isXChanged, xcmd:=xcmd, [readonly]:=[readonly])
            Else
                Call CoreMessageHandler(message:="field entry not found", arg1:=objectname & "." & entryname, messagetype:=otCoreMessageType.InternalError,
                                         subname:="clsOTDBXChangeConfig.addAttributeByField")

                Return False
            End If

        End Function
        '*** add a Attribute by an ID
        '***
        Public Function AddAttributeByField(ByRef objectentry As ObjectEntryDefinition,
                                        Optional ByVal ordinal As Object = Nothing,
                                        Optional ByVal objectname As String = "",
                                        Optional ByVal isxchanged As Boolean = True,
                                        Optional ByVal xcmd As otXChangeCommandType = Nothing,
                                        Optional ByVal [readonly] As Boolean = False) As Boolean
            Dim aMember As XConfigAttributeEntry
            'Dim FIELDENTRY As New clsOTDBSchemaDefTableEntry
            Dim aVAlue As Object
            Dim objectMember As XConfigObjectEntry

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                AddAttributeByField = False
                Exit Function
            End If

            ' load
            If Not objectentry.IsLoaded And Not objectentry.IsCreated Then
                AddAttributeByField = False
                Exit Function
            End If

            ' if ordinal is missing -> create one
            If ordinal Is Nothing Then
                For Each [alias] In objectentry.Aliases
                    'could be more than one Attribute by Alias
                    aMember = Me.AttributeByID(ID:=[alias])
                    If aMember IsNot Nothing Then
                        If aMember.IsLoaded Or aMember.IsCreated Then
                            ordinal = aMember.ordinal
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
                If IsMissing(isxchanged) Then
                    isxchanged = False
                End If
            End If

            '*** Add the Object if necessary
            If objectname = "" Then
                objectMember = Me.ObjectByName(objectentry.Objectname)
                If objectMember Is Nothing Then
                    If Me.AddObjectByName(name:=objectentry.Objectname, xcmd:=xcmd) Then
                        objectMember = Me.ObjectByName(objectentry.Objectname)
                    End If
                End If
            Else
                objectMember = Me.ObjectByName(objectname)
                If objectMember Is Nothing Then
                    If Me.AddObjectByName(name:=objectname, xcmd:=xcmd) Then
                        objectMember = Me.ObjectByName(objectname)
                    End If
                End If
            End If

            '** add a default command -> might be also 0 if object was added with entry
            If xcmd = 0 Then
                xcmd = objectMember.XChangeCmd
            End If


            ' add the component
            aMember = New XConfigAttributeEntry
            If aMember.Create(Me.Configname, Me.GetMaxIndexNo + 1) Then
                aMember.ID = objectentry.ID
                If Not TypeOf ordinal Is OnTrack.Ordinal Then
                    ordinal = New Ordinal(ordinal)
                End If

                aMember.ordinal = ordinal ' create an ordinal 
                aMember.Entryname = objectentry.name
                aMember.IsXChanged = isxchanged
                aMember.IsReadOnly = [readonly]
                aMember.[ObjectEntryDefinition] = objectentry
                aMember.Objectname = objectMember.Objectname
                aMember.XChangeCmd = xcmd
                ' add the Object too
                Return Me.AddMember(aMember)

            End If

            Return False


        End Function
        ''' <summary>
        ''' Adds an Attribute to the XCHange Config by its XChange-ID
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="ordinal"></param>
        ''' <param name="objectname"></param>
        ''' <param name="isXChanged"></param>
        ''' <param name="xcmd"></param>
        ''' <param name="readonly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddAttributeByID(ByVal id As String,
                                            Optional ByVal ordinal As Object = Nothing,
                                            Optional ByVal objectname As String = "",
                                            Optional ByVal isXChanged As Boolean = True,
                                            Optional ByVal xcmd As otXChangeCommandType = Nothing,
                                            Optional ByVal [readonly] As Boolean = False) As Boolean


            AddAttributeByID = False

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                AddAttributeByID = False
                Exit Function
            End If

            '*** no objectname -> get all IDs in objects
            If objectname = "" Then
                For Each entry In CurrentSession.Objects.GetEntryByID(id:=id)
                    '** compare to objects in order
                    If Me.NoObjects > 0 Then
                        For Each anObjectEntry In Me.ObjectsByOrderNo
                            If LCase(entry.Objectname) = LCase(anObjectEntry.Objectname) Then
                                AddAttributeByID = AddAttributeByField(objectentry:=entry, ordinal:=ordinal,
                                                                  isxchanged:=isXChanged,
                                                                  objectname:=entry.Objectname,
                                                                  xcmd:=xcmd, readonly:=[readonly])
                            End If
                        Next
                        ' simply add

                    Else
                        AddAttributeByID = AddAttributeByField(objectentry:=entry, ordinal:=ordinal,
                                                          isxchanged:=isXChanged,
                                                          objectname:=entry.Objectname, xcmd:=xcmd, readonly:=[readonly])
                    End If

                Next

            Else
                For Each entry In CurrentSession.Objects.GetEntryByID(id:=id)
                    If LCase(objectname) = LCase(entry.Objectname) Then
                        AddAttributeByID = AddAttributeByField(objectentry:=entry, ordinal:=ordinal,
                                                          isxchanged:=isXChanged,
                                                          objectname:=entry.Objectname,
                                                          xcmd:=xcmd, readonly:=[readonly])
                    End If
                Next


            End If

            ' return
            AddAttributeByID = AddAttributeByID Or False
            Exit Function


        End Function
        ''' <summary>
        ''' returns True if an Objectname with an ID exists
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Exists(Optional ByVal objectname As String = "", Optional ByVal ID As String = "") As Boolean
            Dim flag As Boolean

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                Exists = False
                Exit Function
            End If

            ' missing arguments
            If objectname = "" Then
                Call CoreMessageHandler(subname:="clsOTDBXChangeConfig.exists", message:="objectname was not set")
                Exists = False
                Exit Function
            End If
            ' missing arguments
            If objectname = "" And ID = "" Then
                Call CoreMessageHandler(subname:="clsOTDBXChangeConfig.exists", message:="set either objectname or attributename - not both")
                Exists = False
                Exit Function
            End If

            '+ check
            If objectname <> "" And ID = "" Then
                If _objectsDirectory.ContainsKey(key:=objectname) Then
                    Exists = True
                Else
                    Exists = False
                End If
                Exit Function
            Else
                If _attributesIDDirectory.ContainsKey(key:=ID) Then

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
        Private Function AddIDReference(ByRef attribute As XConfigAttributeEntry) As Boolean
            Dim entries As List(Of XConfigAttributeEntry)

            If _attributesIDList.ContainsKey(key:=UCase(Attribute.ID)) Then
                entries = _attributesIDList.Item(UCase(Attribute.ID))
            Else

                entries = New List(Of XConfigAttributeEntry)
                _attributesIDList.Add(UCase(Attribute.ID), entries)
            End If
            If entries.Contains(Attribute) Then entries.Remove(Attribute)
            entries.Add(Attribute)

            Return True
        End Function
        ''' <summary>
        ''' Add ordinal to Reference Structures
        ''' </summary>
        ''' <param name="member"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AddordinalReference(ByRef member As iConfigMember) As Boolean
            Dim entries As List(Of iConfigMember)
            '** sorted
            If _membersByordinal.ContainsKey(key:=member.ordinal) Then
                entries = _membersByordinal.Item(member.ordinal)
            Else
                entries = New List(Of iConfigMember)
                _membersByordinal.Add(member.ordinal, entries)
            End If

            If entries.Contains(member) Then entries.Remove(member)
            entries.Add(member)

            Return True
        End Function
        ''' <summary>
        ''' Add an Entry by ID
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AddObjectReference(ByRef member As iConfigMember) As Boolean
            Dim entries As List(Of XConfigAttributeEntry)

            If _attributesByObjectnameDirectory.ContainsKey(key:=LCase(member.Objectname)) Then
                entries = _attributesByObjectnameDirectory.Item(LCase(member.Objectname))
            Else
                entries = New List(Of XConfigAttributeEntry)
                _attributesByObjectnameDirectory.Add(LCase(member.Objectname), entries)
            End If

            entries.Add(member)

            Return True
        End Function
        ''' <summary>
        ''' Add an Entry by ID
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AddAliasReference(ByRef attribute As XConfigAttributeEntry) As Boolean
            Dim entries As List(Of XConfigAttributeEntry)

            For Each [alias] As String In Attribute.Aliases

                If _aliasDirectory.ContainsKey(key:=UCase([alias])) Then
                    entries = _aliasDirectory.Item(key:=UCase([alias]))
                Else
                    entries = New List(Of XConfigAttributeEntry)
                    _aliasDirectory.Add(key:=UCase([alias]), value:=entries)
                End If
                If entries.Contains(Attribute) Then entries.Remove(Attribute)
                entries.Add(Attribute)
            Next

            Return True
        End Function
        ''' <summary>
        ''' Add XChangeMember
        ''' </summary>
        ''' <param name="anEntry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddMember(anEntry As iConfigMember) As Boolean
            Dim anObjectEntry As New XConfigObjectEntry


            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                AddMember = False
                Exit Function
            End If

            ' remove and overwrite
            If _members.ContainsKey(key:=anEntry.Indexno) Then
                Call _members.Remove(key:=anEntry.Indexno)
            End If

            ' add Member Entry
            _members.Add(key:=anEntry.Indexno, value:=anEntry)

            ' Add to the Attribute Section
            If anEntry.IsAttributeEntry Then
                ' check on the Object of the Attribute
                If _objectsDirectory.ContainsKey(key:=anEntry.Objectname) Then
                    anObjectEntry = _objectsDirectory.Item(key:=anEntry.Objectname)
                Else
                    anObjectEntry = New XConfigObjectEntry
                    Call anObjectEntry.Create(Me.Configname, Me.GetMaxIndexNo + 1)
                    anObjectEntry.Objectname = anEntry.Objectname
                    anObjectEntry.Orderno = Me.GetMaxObjectOrderNo + 1
                    anObjectEntry.XChangeCmd = otXChangeCommandType.Read
                    ' add the object entry
                    If Not AddMember(anObjectEntry) Then
                    End If
                End If

                ' add the Attribute
                If _attributesIDDirectory.ContainsKey(key:=anEntry.ID) Then
                    Call _attributesIDDirectory.Remove(key:=anEntry.ID)
                End If

                Call _attributesIDDirectory.Add(key:=anEntry.ID, value:=anEntry)
                '** references
                AddIDReference(anEntry) '-> List references if multipe
                AddObjectReference(anEntry)
                AddAliasReference(anEntry)
                AddordinalReference(anEntry)
                ' Add to the Object Section
            ElseIf anEntry.IsObjectEntry Then
                anObjectEntry = TryCast(anEntry, XConfigObjectEntry)
                If anObjectEntry Is Nothing Then
                    CoreMessageHandler(message:="anEntry is not an ObjectEntry", messagetype:=otCoreMessageType.InternalError,
                                        subname:="XConfig.Addmember")
                    anObjectEntry = anEntry
                End If
                '**
                If _objectsDirectory.ContainsKey(key:=anObjectEntry.Objectname) Then
                    Call _objectsDirectory.Remove(key:=anObjectEntry.Objectname)
                End If
                Call _objectsDirectory.Add(key:=anObjectEntry.Objectname, value:=anEntry)

                '**
                If _objectsByOrderDirectory.ContainsKey(key:=anObjectEntry.Orderno) Then
                    Call _objectsByOrderDirectory.Remove(key:=anObjectEntry.Orderno)
                End If
                Call _objectsByOrderDirectory.Add(key:=anObjectEntry.Orderno, value:=anEntry)
            End If

            '
            AddMember = True

        End Function
        ''' <summary>
        ''' Initialize
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean
            Initialize = MyBase.Initialize()
            _members = New SortedDictionary(Of Long, iConfigMember)
            _attributesIDDirectory = New Dictionary(Of String, XConfigAttributeEntry)
            _objectsDirectory = New Dictionary(Of String, XConfigObjectEntry)
            _DynamicAttributes = False
            _description = ""
            _configname = ""
            _processedDate = ConstNullDate

        End Function
        ''' <summary>
        ''' Resets all dynamic structures
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function Reset() As Boolean
            _objectsDirectory.Clear()
            _objectsByOrderDirectory.Clear()
            _attributesIDDirectory.Clear()
            _attributesByObjectnameDirectory.Clear()
            _attributesIDList.Clear()
            _aliasDirectory.Clear()
            _members.Clear()
            _membersByordinal.Clear()
        End Function
        ''' <summary>
        ''' deletes an objects in persistency store
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Delete() As Boolean
            Dim anEntry As iConfigMember


            If Not Me.IsCreated And Not _IsLoaded Then
                Delete = False
                Exit Function
            End If

            ' delete each entry
            For Each anEntry In _members.Values
                anEntry.Delete()
            Next
            MyBase.Delete()

            ' reset it
            Me.Reset()

            _IsCreated = True
            Me.IsDeleted = True
            Me.Unload()

        End Function

        ''' <summary>
        ''' retrieves an Object by its name or nothing
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ObjectByName(ByVal objectname As String) As XConfigObjectEntry

            If _objectsDirectory.ContainsKey(LCase(objectname)) Then
                Return _objectsDirectory.Item(key:=LCase(objectname))
            Else
                Return Nothing
            End If

        End Function
        '**** ObjectOrderno returns an Object array of orderno's
        '****
        Public Function ObjectOrderNumbers() As IEnumerable
            Return _objectsByOrderDirectory.Keys.ToList
        End Function
        ''' <summary>
        ''' retrieves a list of the Index Numbers of the members
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function MemberIndexNo() As List(Of Long)

            If Not Me.IsCreated And Not _IsLoaded Then
                Return New List(Of Long)
            End If

            Return _members.Keys.ToList

        End Function
        ''' <summary>
        ''' retrieves the ordinal numbers of the objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ObjectsByOrderNo() As IEnumerable(Of XConfigObjectEntry)


            If Not Me.IsCreated And Not _IsLoaded Then
                Return New List(Of XConfigObjectEntry)
            End If

            Return _objectsByOrderDirectory.Values
        End Function

        ''' <summary>
        ''' retrieves a List of Attributes per Objectname
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AttributesByObjectName(ByVal objectname As String) As IEnumerable(Of XConfigAttributeEntry)

            If _attributesByObjectnameDirectory.ContainsKey(objectname) Then
                Return _attributesByObjectnameDirectory.Item(key:=objectname)
            Else
                Return New List(Of XConfigAttributeEntry)
            End If


        End Function

        '**** Members returns a Collection of Members in Order of the IndexNo
        '****
        Public Function MembersByIndexNo() As IEnumerable

            Return Me._members.Values
        End Function

        '**** Members returns a Collection of Members
        '****
        Public Function Members() As List(Of iConfigMember)
            Dim aCollection As New List(Of iConfigMember)

            If Not Me.IsCreated And Not _IsLoaded Then
                Return aCollection
                Exit Function
            End If


            For Each anEntry As clsOTDBXChangeMember In _members.Values
                If (anEntry.ID <> "") And (anEntry.Objectname <> "") Then
                    aCollection.Add(anEntry)
                End If
            Next


            Return aCollection
        End Function
        ''' <summary>
        ''' returns all the objectMembers
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function [Objects]() As List(Of XConfigObjectEntry)
            Dim aCollection As New List(Of XConfigObjectEntry)

            If Not Me.IsCreated And Not _IsLoaded Then
                Return aCollection
            End If

            For Each anEntry As XConfigObjectEntry In _objectsDirectory.Values
                If (anEntry.Objectname <> "") Then
                    aCollection.Add(anEntry)
                End If
            Next

            Return aCollection
        End Function
        ''' <summary>
        ''' returns an attribute by its fieldname and tablename
        ''' </summary>
        ''' <param name="fieldname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AttributeByFieldName(ByVal fieldname As String,
                                            Optional ByVal tablename As String = "") As XConfigAttributeEntry

            Dim aMember As XConfigAttributeEntry
            If Not Me.IsCreated And Not _IsLoaded Then
                AttributeByFieldName = Nothing
                Exit Function
            End If
            Dim alist As List(Of XConfigAttributeEntry)
            If tablename <> "" Then

                '* might be we have the object but no fields
                If _attributesByObjectnameDirectory.ContainsKey(key:=LCase(tablename)) Then
                    alist = _attributesByObjectnameDirectory.Item(key:=LCase(tablename))
                    aMember = alist.Find(Function(m As XConfigAttributeEntry)
                                             Return LCase(m.Entryname) = LCase(fieldname)
                                         End Function)

                    If Not aMember Is Nothing Then
                        Return aMember
                    End If
                End If

            Else
                For Each objectdef In _objectsByOrderDirectory.Values
                    If _attributesByObjectnameDirectory.ContainsKey(key:=objectdef.Objectname) Then
                        alist = _attributesByObjectnameDirectory(key:=objectdef.Objectname)

                        aMember = alist.Find(Function(m As XConfigAttributeEntry)
                                                 Return LCase(m.Entryname) = LCase(fieldname)
                                             End Function)

                        If Not aMember Is Nothing Then
                            Return aMember
                        End If
                    End If
                Next
            End If

            '** search also by ID and consequent by ALIAS
            Dim anObjectEntry As ObjectEntryDefinition = CurrentSession.Objects.GetEntry(objectname:=tablename, entryname:=fieldname)
            If Not anObjectEntry Is Nothing Then
                aMember = Me.AttributeByID(ID:=anObjectEntry.ID, objectname:=tablename)
                If Not aMember Is Nothing Then
                    Return aMember
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
        Public Function AttributeByID(ByVal ID As String,
                                        Optional ByVal objectname As String = "") As XConfigAttributeEntry

            Dim aCollection As IEnumerable


            If Not Me.IsCreated And Not _IsLoaded Then
                AttributeByID = Nothing
                Exit Function
            End If

            If _attributesIDList.ContainsKey(UCase(ID)) Then
                aCollection = _attributesIDList.Item(UCase(ID))
                For Each entry As XConfigAttributeEntry In aCollection
                    If objectname <> "" AndAlso LCase(entry.Objectname) = LCase(objectname) Then
                        Return entry
                    ElseIf objectname = "" Then
                        Return entry
                    End If
                Next

            End If

            '** look into aliases 
            '**
            '* check if ID is an ID already in the xconfig
            AttributeByID = AttributeByAlias(ID, objectname)
            If AttributeByID Is Nothing Then
                '* check all Objects coming through with this ID
                For Each anObjectEntry In CurrentSession.Objects.GetEntryByID(id:=ID)
                    '** check on all the XConfig Objects
                    For Each anObjectMember In Me.ObjectsByOrderNo
                        '* if ID is included as Alias Name
                        AttributeByID = AttributeByAlias(alias:=anObjectEntry.ID, objectname:=anObjectMember.Objectname)
                        '** or the aliases are included in this XConfig
                        If AttributeByID Is Nothing Then
                            For Each aliasID In anObjectEntry.Aliases
                                AttributeByID = AttributeByAlias(alias:=aliasID, objectname:=anObjectMember.Objectname)
                                '* found
                                If Not AttributeByID Is Nothing Then
                                    Exit For
                                End If
                            Next

                        End If
                        '* found
                        If Not AttributeByID Is Nothing Then
                            Exit For
                        End If
                    Next
                    '* found
                    If Not AttributeByID Is Nothing Then
                        Exit For
                    End If
                Next

            End If
            Return AttributeByID
        End Function
        ''' <summary>
        ''' returns a List of XConfigMembers per ordinal
        ''' </summary>
        ''' <param name="ordinal"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AttributesByordinal(ByVal ordinal As Ordinal) As List(Of iConfigMember)

            If Not Me.IsCreated And Not _IsLoaded Then
                Return New List(Of iConfigMember)
            End If

            If _membersByordinal.ContainsKey(ordinal) Then
                Return _membersByordinal.Item(ordinal)
            Else
                Return New List(Of iConfigMember)
            End If

        End Function
        ''' <summary>
        ''' returns an Attribute by its XChange Alias ID
        ''' </summary>
        ''' <param name="alias"></param>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AttributeByAlias(ByVal [alias] As String,
                                        Optional ByVal objectname As String = "") As XConfigAttributeEntry

            Dim aCollection As IEnumerable


            If Not Me.IsCreated And Not _IsLoaded Then
                AttributeByAlias = Nothing
                Exit Function
            End If

            If _aliasDirectory.ContainsKey(UCase([alias])) Then

                aCollection = _aliasDirectory.Item(UCase([alias]))
                For Each entry As XConfigAttributeEntry In aCollection
                    If objectname <> "" AndAlso LCase(entry.Objectname) = LCase(objectname) Then
                        Return entry
                    ElseIf objectname = "" Then
                        Return entry
                    End If
                Next

            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' Returns an ienumerable of all attributes (optional just by an objectname)
        ''' </summary>
        ''' <param name="objectname">optional objectname</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Attributes(Optional objectname As String = "") As IEnumerable(Of XConfigAttributeEntry)
            If Not Me.IsCreated And Not _IsLoaded Then
                Return New List(Of clsOTDBXChangeMember)
            End If

            If objectname <> "" Then
                Return AttributesByObjectName(objectname)
            Else
                Return _attributesIDDirectory.Values.ToList
            End If

        End Function
        ''' <summary>
        ''' Loads a XChange Configuration from Store
        ''' </summary>
        ''' <param name="configname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal configname As String) As Boolean
            Dim aTable As iormDataStore
            Dim aRecordCollection As List(Of ormRecord)
            Dim aRecord As ormRecord
            Dim anEntry As iConfigMember

            Dim pkarry() As Object = {LCase(configname)}

            '* lazy init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    LoadBy = False
                    Exit Function
                End If
            End If

            Try
                ' set the primaryKey
                '** load the object itself
                If MyBase.LoadBy(pkarry) Then
                    ' load the members
                    aTable = GetTableStore(XConfigMember.ConstTableID)
                    Dim aCommand As ormSqlSelectCommand = aTable.CreateSqlSelectCommand(id:="loadbyXConfig")
                    If Not aCommand.Prepared Then
                        aCommand.Where = XConfigMember.ConstFNID & " = @" & XConfigMember.ConstFNID
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@" & constFNID, fieldname:=XConfigMember.ConstFNID, tablename:=XConfigMember.ConstTableID))
                        aCommand.OrderBy = "[" & XConfigMember.ConstTableID & "." & XConfigMember.ConstFNIDNo & "] asc"
                        aCommand.Prepare()
                    End If
                    If aCommand.Prepared Then
                        aCommand.SetParameterValue(ID:="@" & XConfigMember.ConstFNID, value:=configname)
                    End If
                    aRecordCollection = aCommand.RunSelect

                    ' record collection
                    _configname = configname

                    ' records read
                    For Each aRecord In aRecordCollection
                        ' add the Entry as Component (even the header -> )
                        If aRecord.GetValue(XConfigMember.constFNIsAttributeEntry) Then
                            anEntry = New XConfigAttributeEntry
                        ElseIf aRecord.GetValue(XConfigMember.constFNIsObjectEntry) Then
                            anEntry = New XConfigObjectEntry
                        Else
                            anEntry = Nothing
                            CoreMessageHandler(message:="Member is not determineable if object or attribute", messagetype:=otCoreMessageType.InternalError,
                                                subname:="Xconfig.loadby")
                        End If

                        If anEntry IsNot Nothing AndAlso anEntry.Infuse(aRecord) Then
                            If Not Me.AddMember(anEntry) Then
                                CoreMessageHandler(message:="couldnot add member", subname:="XConfig.loadby",
                                                   messagetype:=otCoreMessageType.InternalError)
                            End If
                        End If
                    Next
                    '
                    _IsLoaded = True

                End If

                Return True


            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="clsOTDBXchangConfig.Loadby")
                Me.Unload()
                Return False
            End Try


        End Function

        ''' <summary>
        ''' perstist the XChange Config to the datastore
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As DateTime = ConstNullDate) As Boolean
            Persist = MyBase.Persist
            If Persist Then
                ' persist each entry
                If _members.Count > 0 Then
                    For Each anEntry In _members.Values
                        Persist = Persist And anEntry.Persist(timestamp)
                    Next
                End If
                Return Persist
            End If

            Return False

        End Function

        ''' <summary>
        ''' create a persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateSchema(Of XConfig)()
        End Function
        ''' <summary>
        ''' creates a persistable object with primary key
        ''' </summary>
        ''' <param name="configname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal configname As String) As Boolean
            Dim primarykey() As Object = {LCase(configname)}

            If MyBase.Create(primarykey, checkUnique:=True) Then
                ' set the primaryKey
                _configname = LCase(configname)
            End If

            Return Me.IsCreated
        End Function

#Region "Static functions"

        ''' <summary>
        ''' retrieves a List of all XConfigs
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function All() As List(Of XConfig)
            Dim aList As New List(Of XConfig)
            Dim aRecordCollection As New List(Of ormRecord)
            Dim aTable As iormDataStore

            Dim aRecord As ormRecord

            Try
                aTable = GetTableStore(constTableID)
                Dim aCommand As ormSqlSelectCommand = aTable.CreateSqlSelectCommand(id:="All")
                If Not aCommand.Prepared Then
                    aCommand.Prepare()
                End If

                aRecordCollection = aCommand.RunSelect

                For Each aRecord In aRecordCollection
                    Dim aNewObject As New XConfig
                    aNewObject = New XConfig
                    If aNewObject.Infuse(aRecord) Then
                        ' loadby to get all items
                        If aNewObject.LoadBy(aNewObject.Configname) Then
                            aList.Add(item:=aNewObject)
                        End If
                    End If
                Next aRecord

                Return aList

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="XConfig.All")
                Return aList
            End Try
        End Function
#End Region
    End Class

    ''' <summary>
    ''' XBag is an arbitary XChange Data Object which constists of different XEnvelopes ordered by
    ''' ordinals.
    ''' An XBag an Default persistable XChangeConfig
    ''' </summary>
    ''' <remarks></remarks>
    Public Class XBag
        Implements IEnumerable(Of XBag)

        '* default Config we are looking over
        Private _XChangeDefaultConfig As XConfig
        Private _XCmd As otXChangeCommandType = 0

        '* real Attributes used after prepared
        Private _usedAttributes As New Dictionary(Of String, iConfigMember)
        Private _usedObjects As New Dictionary(Of String, iConfigMember)

        '** all the member envelopes
        Private WithEvents _defaultEnvelope As New XEnvelope(Me)
        Private WithEvents _envelopes As New SortedDictionary(Of Ordinal, XEnvelope)

        '** flags

        Private _isPrepared As Boolean = False

        Private _PreparedOn As Date = ConstNullDate

        Private _IsPrechecked As Boolean = False
        Private _PrecheckedOk As Boolean = False
        Private _PrecheckTimestamp As Date = ConstNullDate

        Private _isProcessed As Boolean = False
        Private _XChangedOK As Boolean = False
        Private _ProcessedTimestamp As Date = ConstNullDate


        '** events for convert values
        Public Event ConvertRequest2HostValue As EventHandler(Of ConvertRequestEventArgs)
        Public Event ConvertRequest2DBValue As EventHandler(Of ConvertRequestEventArgs)


        Public Sub New(xchangeDefaultConfig As XConfig)
            _XChangeDefaultConfig = xchangeDefaultConfig

        End Sub


#Region "Properties"

        ''' <summary>
        ''' Gets the default envelope.
        ''' </summary>
        ''' <value>The default envelope.</value>
        Public ReadOnly Property DefaultEnvelope() As XEnvelope
            Get
                Return Me._defaultEnvelope
            End Get
        End Property

        Public ReadOnly Property IsPrechecked As Boolean
            Get
                Return _IsPrechecked
            End Get
        End Property
        Public ReadOnly Property PrecheckedOk As Boolean
            Get
                Return _PrecheckedOk
            End Get
        End Property
        Public ReadOnly Property PrecheckTimestamp As Date
            Get
                Return _PrecheckTimestamp
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the top CMD.
        ''' </summary>
        ''' <value>The top CMD.</value>
        Public Property XChangeCommand() As otXChangeCommandType
            Get
                Return Me._XCmd
            End Get
            Set(value As otXChangeCommandType)
                Me._XCmd = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the prepared on.
        ''' </summary>
        ''' <value>The prepared on.</value>
        Public Property PreparedOn() As DateTime
            Get
                Return Me._PreparedOn
            End Get
            Private Set(value As DateTime)
                Me._PreparedOn = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the processed on.
        ''' </summary>
        ''' <value>The processed on.</value>
        Public Property ProcessedOn() As DateTime
            Get
                Return Me._ProcessedTimestamp
            End Get
            Private Set(value As DateTime)
                Me._ProcessedTimestamp = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the is prepared.
        ''' </summary>
        ''' <value>The is prepared.</value>
        Public Property IsPrepared() As Boolean
            Get
                Return _isPrepared
            End Get
            Private Set(value As Boolean)
                _isPrepared = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the is processed.
        ''' </summary>
        ''' <value>The is processed.</value>
        Public Property IsProcessed() As Boolean
            Get
                Return Me._isProcessed
            End Get
            Private Set(value As Boolean)
                Me._isProcessed = value
            End Set
        End Property
        ''' <summary>
        ''' returns true if the successfully processed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ProcessedOK As Boolean
            Get
                Return _XChangedOK
            End Get
        End Property
        ''' <summary>
        ''' Gets the xchangeconfig.
        ''' </summary>
        ''' <value>The xchangeconfig.</value>
        Public ReadOnly Property XChangeDefaultConfig() As XConfig
            Get
                Return Me._XChangeDefaultConfig
            End Get
        End Property

#End Region

#Region "Administration functions"

        Public Function ordinals() As System.Collections.Generic.SortedDictionary(Of Ordinal, XEnvelope).KeyCollection
            Return _envelopes.Keys
        End Function
        '**** check functions if exists
        Public Function ContainsKey(ByVal key As Ordinal) As Boolean
            Return Me.Hasordinal(key)
        End Function
        Public Function ContainsKey(ByVal key As Long) As Boolean
            Return Me.Hasordinal(New Ordinal(key))
        End Function
        Public Function ContainsKey(ByVal key As String) As Boolean
            Return Me.Hasordinal(New Ordinal(key))
        End Function
        Public Function Hasordinal(ByVal ordinal As Ordinal) As Boolean
            Return _envelopes.ContainsKey(ordinal)
        End Function

        '***** remove 
        Public Function RemoveEnvelope(ByVal key As Long) As Boolean
            Me.RemoveEnvelope(New Ordinal(key))
        End Function
        Public Function RemoveEnvelope(ByVal key As String) As Boolean
            Me.RemoveEnvelope(New Ordinal(key))
        End Function
        Public Function RemoveEnvelope(ByVal ordinal As Ordinal) As Boolean
            If Me.Hasordinal(ordinal) Then
                Dim envelope = _envelopes.Item(key:=ordinal)
                '** add handlers
                RemoveHandler envelope.ConvertRequest2HostValue, AddressOf Me.OnRequestConvert2HostValue
                RemoveHandler envelope.ConvertRequestDBValue, AddressOf Me.OnRequestConvert2DBValue
                _envelopes.Remove(ordinal)
                Return True
            End If
            Return False
        End Function

        ''' <summary>
        ''' clear all entries remove all envelopes
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Clear() As Boolean
            _defaultEnvelope.Clear()
            For Each ordinal In _envelopes.Keys
                RemoveEnvelope(ordinal:=ordinal)
            Next
            _envelopes.Clear()
            If _envelopes.Count > 0 Then Return False
            Return True
        End Function
        '***** function to add an Entry
        ''' <summary>
        ''' adds an envelope to the bag by key
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="envelope"></param>
        ''' <param name="removeIfExists"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddEnvelope(ByVal key As Long, Optional ByVal envelope As XEnvelope = Nothing, Optional removeIfExists As Boolean = True) As XEnvelope
            Return Me.AddEnvelope(ordinal:=New Ordinal(key), envelope:=envelope, removeIfExists:=removeIfExists)
        End Function
        ''' <summary>
        ''' adds an envelope to the bag by key
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="envelope"></param>
        ''' <param name="removeIfExists"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddEnvelope(ByVal key As String, Optional ByVal envelope As XEnvelope = Nothing, Optional removeIfExists As Boolean = True) As XEnvelope
            Return Me.AddEnvelope(ordinal:=New Ordinal(key), envelope:=envelope, removeIfExists:=removeIfExists)
        End Function
        ''' <summary>
        ''' adds an envelope to the bag by ordinal
        ''' </summary>
        ''' <param name="ordinal"></param>
        ''' <param name="envelope"></param>
        ''' <param name="removeIfExists"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddEnvelope(ByVal ordinal As Ordinal, Optional ByVal envelope As XEnvelope = Nothing, Optional removeIfExists As Boolean = False) As XEnvelope
            If Me.Hasordinal(ordinal) Then
                'TODO: Differentiate if the value is coming from which object -> donot overwrite with wrong information
                If removeIfExists Then
                    Me.RemoveEnvelope(ordinal)
                Else
                    Return Nothing
                End If
            End If
            If envelope Is Nothing Then
                envelope = New XEnvelope(Me)
            End If
            '** add handlers -> done in new of XEnvelope
            'AddHandler envelope.ConvertRequest2HostValue, AddressOf Me.OnRequestConvert2HostValue
            'AddHandler envelope.ConvertRequestDBValue, AddressOf Me.OnRequestConvert2DBValue
            'add it
            _envelopes.Add(ordinal, value:=envelope)
            Return envelope
        End Function

        '***** replace
        ''' <summary>
        ''' replaces or adds an envelope against another with same key
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="envelope"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ReplaceEnvelope(ByVal key As Long, ByVal envelope As XEnvelope) As XEnvelope
            Return Me.AddEnvelope(ordinal:=New Ordinal(key), envelope:=envelope, removeIfExists:=True)
        End Function
        ''' <summary>
        ''' replaces or adds an envelope against another with same key
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="envelope"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ReplaceEnvelope(ByVal key As String, ByVal envelope As XEnvelope) As XEnvelope
            Return Me.AddEnvelope(ordinal:=New Ordinal(key), envelope:=envelope, removeIfExists:=True)
        End Function
        ''' <summary>
        ''' replaces or adds an envelope against another with same ordinal
        ''' </summary>
        ''' <param name="ordinal"></param>
        ''' <param name="envelope"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ReplaceEnvelope(ByVal ordinal As Ordinal, ByVal envelope As XEnvelope) As XEnvelope
            Return Me.AddEnvelope(ordinal:=ordinal, envelope:=envelope, removeIfExists:=True)
        End Function
        ''' <summary>
        ''' returns an Envelope by key
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Item(ByVal key As Object) As XEnvelope
            If TypeOf key Is Ordinal Then
                Dim ordinal As Ordinal = DirectCast(key, Ordinal)
                Return Me.GetEnvelope(ordinal:=ordinal)
            ElseIf IsNumeric(key) Then
                Return Me.GetEnvelope(key:=CLng(key))
            Else
                Return Me.GetEnvelope(key:=key.ToString)
            End If

        End Function
        ''' <summary>
        ''' returns an Envelope by key
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEnvelope(ByVal key As Long) As XEnvelope
            Return Me.GetEnvelope(ordinal:=New Ordinal(key))
        End Function
        ''' <summary>
        '''  returns an Envelope by key
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEnvelope(ByVal key As String) As XEnvelope
            Return Me.GetEnvelope(ordinal:=New Ordinal(key))
        End Function
        ''' <summary>
        '''  returns an Envelope by ordinal
        ''' </summary>
        ''' <param name="ordinal"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEnvelope(ByVal ordinal As Ordinal) As XEnvelope
            If _envelopes.ContainsKey(key:=ordinal) Then
                Return _envelopes.Item(key:=ordinal)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' gets an enumarator over the envelopes
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEnumerator() As IEnumerator(Of XBag) Implements IEnumerable(Of XBag).GetEnumerator
            _envelopes.GetEnumerator()
        End Function

        Public Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            _envelopes.GetEnumerator()
        End Function
#End Region

        ''' <summary>
        ''' Event handler for the Slots OnRequestConvert2Hostvalue
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnRequestConvert2HostValue(ByVal sender As Object, ByVal e As ConvertRequestEventArgs) Handles _defaultEnvelope.ConvertRequest2HostValue
            RaiseEvent ConvertRequest2HostValue(sender, e) ' cascade
        End Sub
        ''' <summary>
        ''' EventHandler for the Slots OnRequestConvert2DBValue
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnRequestConvert2DBValue(ByVal sender As Object, ByVal e As ConvertRequestEventArgs) Handles _defaultEnvelope.ConvertRequestDBValue
            RaiseEvent ConvertRequest2DBValue(sender, e) ' cascade
        End Sub
        ''' <summary>
        ''' Prepares the XBag for the Operations to run on it
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Prepare(Optional force As Boolean = False) As Boolean
            If Me.IsPrepared And Not force Then
                Return True
            End If

            If _XCmd = 0 Then
                _XCmd = _XChangeDefaultConfig.GetHighestXCmd()
            End If


            _isPrepared = True
            _PreparedOn = Date.Now
            Return True
        End Function


        ''' <summary>
        ''' Runs the XChange PreCheck
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunPreXCheck() As Boolean

            RunPreXCheck = True

            ' Exchange all Envelopes
            For Each anEnvelope In _envelopes.Values
                RunPreXCheck = RunPreXCheck And anEnvelope.RunXPreCheck
            Next

            _IsPrechecked = True
            _PrecheckedOk = RunPreXCheck
            _PrecheckTimestamp = Date.Now

            Return RunPreXCheck
        End Function
        ''' <summary>
        ''' Runs the XChange
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunXChange() As Boolean

            RunXChange = True

            ' Exchange all Envelopes
            For Each anEnvelope In _envelopes.Values
                RunXChange = RunXChange And anEnvelope.runXChange
            Next

            _XChangedOK = RunXChange
            _isProcessed = True
            _ProcessedTimestamp = Date.Now
            Return RunXChange
        End Function
    End Class

    ''' <summary>
    ''' a XSlot represents a Slot in an XEnvelope
    ''' </summary>
    ''' <remarks></remarks>

    Public Class XSlot

        Private _envelope As XEnvelope
        Private _xattribute As XConfigAttributeEntry
        Private _explicitDatatype As otFieldDataType

        Private _ordinal As Ordinal

        Private _hostvalue As Object = Nothing
        Private _isEmpty As Boolean = False
        Private _isNull As Boolean = False
        Private _isPrechecked As Boolean = False
        Private _isPrecheckedOk As Boolean = False


        Private _msglog As New ObjectLog

        '** events for convert values
        Public Event ConvertRequest2HostValue As EventHandler(Of ConvertRequestEventArgs)
        Public Event ConvertRequest2DBValue As EventHandler(Of ConvertRequestEventArgs)

        ''' <summary>
        ''' constructor for slot with envelope reference and attribute
        ''' </summary>
        ''' <param name="xenvelope"></param>
        ''' <param name="attribute"></param>
        ''' <remarks></remarks>
        Public Sub New(xenvelope As XEnvelope, attribute As XConfigAttributeEntry)
            _envelope = xenvelope
            _xattribute = attribute
            _ordinal = attribute.ordinal
            _hostvalue = Nothing
            _isEmpty = True
            _isNull = True
            _explicitDatatype = 0 'read from attribute
            AddHandler Me.ConvertRequest2HostValue, AddressOf xenvelope.OnRequestConvert2HostValue
            AddHandler Me.ConvertRequest2DBValue, AddressOf xenvelope.OnRequestConvert2DBValue
        End Sub
        ''' <summary>
        ''' constructor for slot with envelope reference and attribute and hostvalue
        ''' </summary>
        ''' <param name="xenvelope"></param>
        ''' <param name="attribute"></param>
        ''' <remarks></remarks>
        Public Sub New(xenvelope As XEnvelope, attribute As XConfigAttributeEntry, hostvalue As Object, Optional isEmpty As Boolean = False, Optional isNull As Boolean = False)
            _envelope = xenvelope
            _xattribute = attribute
            _ordinal = attribute.ordinal
            _hostvalue = hostvalue
            _isEmpty = isEmpty
            _isNull = isNull
            _explicitDatatype = 0 'read from attribute
            AddHandler Me.ConvertRequest2HostValue, AddressOf xenvelope.OnRequestConvert2HostValue
            AddHandler Me.ConvertRequest2DBValue, AddressOf xenvelope.OnRequestConvert2DBValue
        End Sub
#Region "Properties"
        ''' <summary>
        ''' gets the pre checked result - only valid if ISPrechecked is true
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsPrecheckedOk As Boolean
            Get
                Return _isPrecheckedOk
            End Get
            Private Set(ByVal value As Boolean)
                _isPrecheckedOk = value
            End Set
        End Property
        ''' <summary>
        ''' returns True if Slot is supposed to be XChanged
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsXChanged As Boolean
            Get
                If _xattribute IsNot Nothing Then
                    Return Not Me.IsEmpty And Me.XAttribute.IsXChanged And Not Me.XAttribute.IsReadOnly
                Else
                    Return Not Me.IsEmpty
                End If
            End Get
        End Property
        ''' <summary>
        ''' gets the IsPrechecked flag if pre check has Run
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsPrechecked As Boolean
            Private Set(value As Boolean)
                _isPrechecked = value
            End Set
            Get
                Return _isPrechecked
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the ordinal.
        ''' </summary>
        ''' <value>The ordinal.</value>
        Public Property ordinal() As Ordinal
            Get
                Return Me._ordinal
            End Get
            Private Set(value As Ordinal)
                Me._ordinal = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the is null.
        ''' </summary>
        ''' <value>The is null.</value>
        Public Property IsNull() As Boolean
            Get
                Return Me._isNull Or IsDBNull(_hostvalue)
            End Get
            Set(value As Boolean)
                Me._isNull = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the is empty.
        ''' </summary>
        ''' <value>The is empty.</value>
        Public Property IsEmpty() As Boolean
            Get
                Return Me._isEmpty
            End Get
            Set(value As Boolean)
                Me._isEmpty = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the host value.
        ''' </summary>
        ''' <value>The value.</value>
        Public Property HostValue() As Object
            Get
                Return Me._hostvalue
            End Get
            Set(value As Object)
                Me._hostvalue = value
                Me.IsEmpty = False ' HACK ! should raise event
                Me.IsNull = False
            End Set
        End Property

        Public Property Datatype As otFieldDataType
            Get
                If _xattribute IsNot Nothing And _explicitDatatype = 0 Then
                    Return _xattribute.ObjectEntryDefinition.Datatype
                ElseIf _explicitDatatype <> 0 Then
                    Return _explicitDatatype
                Else
                    CoreMessageHandler(message:="Attribute or Datatype not set in slot", messagetype:=otCoreMessageType.InternalError, subname:="XSlot.Datatype")
                    Return 0
                End If
            End Get
            Set(value As otFieldDataType)
                If _xattribute Is Nothing Then
                    _explicitDatatype = value
                Else
                    CoreMessageHandler(message:="explicit datatype cannot be set if attribute was specified", messagetype:=otCoreMessageType.InternalWarning, subname:="XSlot.Datatype")
                    _explicitDatatype = value
                End If
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the Database value.
        ''' </summary>
        ''' <value>The value.</value>
        Public Property DBValue() As Object
            Get
                Dim isNull As Boolean = False
                Dim isEmpty As Boolean = False
                Dim outvalue As Object = _hostvalue
                Dim anArgs As New ConvertRequestEventArgs(Datatype:=Me.Datatype, valuetype:=ConvertRequestEventArgs.convertValueType.Hostvalue,
                                                          value:=_hostvalue, isempty:=Me.IsEmpty, isnull:=Me.IsNull)
                '** raise the event if we have a special eventhandler
                RaiseEvent ConvertRequest2DBValue(sender:=Me, e:=anArgs)
                If anArgs.ConvertSucceeded Then
                    Me.IsEmpty = anArgs.HostValueisEmpty
                    Me.IsNull = anArgs.HostValueisNull
                    Return anArgs.Dbvalue
                Else
                    If DefaultConvert2HostValue(datatype:=Me.Datatype, dbvalue:=outvalue, hostvalue:=_hostvalue, _
                                                dbValueIsEmpty:=isEmpty, dbValueIsNull:=isNull, hostValueIsEmpty:=_isEmpty, hostValueIsNull:=_isNull, _
                                                msglog:=Me._msglog) Then
                        Return outvalue

                    Else
                        Return DBNull.Value
                    End If
                End If

            End Get
            Set(value As Object)
                Dim isNull As Boolean = value Is Nothing
                Dim isEmpty As Boolean = False
                Dim outvalue As Object = Nothing
                Dim anArgs As New ConvertRequestEventArgs(Datatype:=Me.Datatype, valuetype:=ConvertRequestEventArgs.convertValueType.DBValue,
                                                          value:=value, isnull:=isNull, isempty:=isEmpty)

                RaiseEvent ConvertRequest2HostValue(sender:=Me, e:=anArgs)
                If anArgs.ConvertSucceeded Then
                    _hostvalue = anArgs.Hostvalue
                    Me.IsEmpty = anArgs.HostValueisEmpty
                    Me.IsNull = anArgs.HostValueisNull
                Else

                    If DefaultConvert2HostValue(datatype:=Me.Datatype, dbvalue:=value, hostvalue:=outvalue, _
                                                dbValueIsEmpty:=Me.IsEmpty, dbValueIsNull:=Me.IsNull, hostValueIsEmpty:=isEmpty, hostValueIsNull:=isNull, _
                                                msglog:=Me._msglog) Then
                        _hostvalue = outvalue
                    End If
                End If

            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the xattribute.
        ''' </summary>
        ''' <value>The xattribute.</value>
        Public Property XAttribute() As XConfigAttributeEntry
            Get
                Return Me._xattribute
            End Get
            Set(value As XConfigAttributeEntry)
                Me._xattribute = value
            End Set
        End Property
#End Region

        ''' <summary>
        ''' convert a value according an objectentry from dbvalue to hostvalue
        ''' </summary>
        ''' <param name="slot"></param>
        ''' <param name="dbvalue"></param>
        ''' <param name="hostvalue"></param>
        ''' <param name="msglog"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Shared Function DefaultConvert2HostValue(ByRef datatype As otFieldDataType,
                                                 ByRef hostvalue As Object, ByVal dbvalue As Object,
                                                Optional ByRef hostValueIsNull As Boolean = False, Optional ByRef hostValueIsEmpty As Boolean = False,
                                                Optional dbValueIsNull As Boolean = False, Optional dbValueIsEmpty As Boolean = False,
                                                Optional ByRef msglog As ObjectLog = Nothing) As Boolean

            ' set msglog
            If msglog Is Nothing Then
                If msglog Is Nothing Then
                    msglog = New ObjectLog
                End If
                'MSGLOG.Create(Me.Msglogtag)
            End If

            '*** transfer
            '****

            hostValueIsEmpty = False
            hostValueIsNull = False

            Select Case datatype
                Case otFieldDataType.[Long]
                    If dbValueIsNull Then
                        hostvalue = CLng(0) ' HACK ! Should be Default Null Value
                        hostValueIsNull = True
                        Return True
                    ElseIf dbValueIsEmpty Then
                        hostvalue = Nothing
                        hostValueIsEmpty = True
                        Return True
                    ElseIf IsNumeric(dbvalue) Then
                        hostvalue = CLng(dbvalue)
                        Return True
                    Else
                        Call CoreMessageHandler(subname:="clsOTDBXChangeMember.convertValue2Hostvalue",
                                              message:="OTDB data " & dbvalue & " is not convertible to long",
                                              arg1:=dbvalue)
                        hostValueIsEmpty = True
                        Return False
                    End If
                Case otFieldDataType.Numeric
                    If dbValueIsNull Then
                        hostvalue = CDbl(0) ' HACK ! Should be Default Null Value
                        hostValueIsNull = True
                        Return True
                    ElseIf dbValueIsEmpty Then
                        hostvalue = Nothing
                        hostValueIsEmpty = True
                        Return True
                    ElseIf IsNumeric(dbvalue) Then
                        hostvalue = CDbl(dbvalue)
                        Return True
                    Else
                        Call CoreMessageHandler(subname:="clsOTDBXChangeMember.convertValue2Hostvalue",
                                              message:="OTDB data " & dbvalue & " is not convertible to double",
                                              arg1:=dbvalue)
                        hostvalue = Nothing
                        hostValueIsEmpty = True
                        Return False
                    End If


                Case otFieldDataType.Text, otFieldDataType.List, otFieldDataType.Memo

                    hostvalue = CStr(dbvalue)
                    Return True

                Case otFieldDataType.Runtime
                    Call CoreMessageHandler(subname:="clsOTDBXChangeMember.convertValue2Hostvalue",
                                            message:="OTDB data " & dbvalue & " is not convertible to runtime",
                                            arg1:=dbvalue)
                    hostvalue = Nothing
                    hostValueIsEmpty = True
                    Return False

                Case otFieldDataType.Formula
                    Call CoreMessageHandler(subname:="clsOTDBXChangeMember.convertValue2Hostvalue",
                                            message:="OTDB data " & dbvalue & " is not convertible to formula",
                                            arg1:=dbvalue)
                    hostvalue = Nothing
                    hostValueIsEmpty = True
                    Return False

                Case otFieldDataType.[Date], otFieldDataType.Time, otFieldDataType.Timestamp
                    If dbValueIsNull OrElse IsDBNull(dbvalue) OrElse dbvalue = ConstNullDate OrElse dbvalue = ConstNullTime Then
                        If datatype = otFieldDataType.Time Then
                            hostvalue = ConstNullTime ' HACK ! Should be Default Null Value
                        Else
                            hostvalue = ConstNullDate
                        End If
                        hostValueIsNull = True
                        Return True
                    ElseIf dbValueIsEmpty Then
                        hostvalue = Nothing
                        hostValueIsEmpty = True
                        Return True
                    ElseIf IsDate(dbvalue) Then
                        hostvalue = dbvalue
                        Return True
                    Else
                        Call CoreMessageHandler(subname:="clsOTDBXChangeMember.convertValue2Hostvalue",
                                              message:="OTDB data " & dbvalue & " is not convertible to date, time, timestamp",
                                              arg1:=dbvalue)
                        hostvalue = Nothing
                        hostValueIsEmpty = True
                        Return False
                    End If

                Case otFieldDataType.Bool
                    hostvalue = dbvalue
                    Return True
                Case otFieldDataType.Binary
                    hostvalue = dbvalue
                    Return True
                Case Else
                    Call CoreMessageHandler(subname:="XSlot.convert2HostValue",
                                           message:="type has no converter",
                                           arg1:=hostvalue)
                    hostvalue = Nothing
                    hostValueIsEmpty = True
                    Return False
            End Select

        End Function



        ''' <summary>
        ''' Default Convert to DBValue without any specials
        ''' </summary>
        ''' <param name="slot"></param>
        ''' <param name="hostvalue"></param>
        ''' <param name="dbvalue"></param>
        ''' <param name="msglog"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function DefaultConvert2DBValue(ByRef datatype As otFieldDataType,
                                                ByVal hostvalue As Object, ByRef dbvalue As Object,
                                                Optional hostValueIsNull As Boolean = False, Optional hostValueIsEmpty As Boolean = False,
                                                Optional ByRef dbValueIsNull As Boolean = False, Optional ByRef dbValueIsEmpty As Boolean = False,
                                                Optional ByRef msglog As ObjectLog = Nothing) As Boolean
            ' set msglog
            If msglog Is Nothing Then
                msglog = New ObjectLog
            End If

            '*** transfer
            '****
            ' default
            dbValueIsEmpty = False
            dbValueIsNull = True

            Select Case datatype

                Case otFieldDataType.Numeric, otFieldDataType.[Long]
                    If hostvalue Is Nothing OrElse hostValueIsNull Then
                        dbvalue = DBNull.Value
                        dbValueIsNull = True
                        Return True
                    ElseIf IsNumeric(hostvalue) Then
                        If datatype = otFieldDataType.Numeric Then
                            dbvalue = CDbl(dbvalue)    ' simply keep it
                            Return True
                        Else
                            dbvalue = CLng(dbvalue)
                            Return True
                        End If
                    Else
                        ' ERROR
                        CoreMessageHandler(message:="value is not convertible to numeric or long", arg1:=hostvalue,
                                           subname:="Xslot.DefaultConvert2DBValue", messagetype:=otCoreMessageType.ApplicationError)
                        dbvalue = Nothing
                        dbValueIsEmpty = True
                        Return False
                    End If


                Case otFieldDataType.Text, otFieldDataType.List, otFieldDataType.Memo

                    If hostvalue Is Nothing Then
                        dbvalue = DBNull.Value
                        dbValueIsNull = True
                        Return True
                    ElseIf True Then
                        dbvalue = CStr(hostvalue)
                        Return True
                    Else
                        ' ERROR
                        CoreMessageHandler(message:="value is not convertible to string", subname:="Xslot.DefaultConvert2DBValue",
                                            messagetype:=otCoreMessageType.ApplicationError)
                        dbvalue = Nothing
                        dbValueIsEmpty = True
                        Return False
                    End If

                Case otFieldDataType.Runtime
                    Call CoreMessageHandler(subname:="XSlot.convert2DBValue",
                                          message:="OTDB data " & hostvalue & " is not convertible from/to runtime",
                                           arg1:=hostvalue)

                    dbvalue = DBNull.Value
                    Return False

                Case otFieldDataType.Formula
                    Call CoreMessageHandler(subname:="XSlot.convert2DBValue", arg1:=hostvalue.ToString,
                                          message:="OTDB data " & hostvalue & " is not convertible from/to formula")

                    dbvalue = Nothing
                    dbValueIsEmpty = True
                    Return False

                Case otFieldDataType.[Date], otFieldDataType.Time, otFieldDataType.Timestamp
                    If hostvalue Is Nothing OrElse hostValueIsNull = True Then
                        dbvalue = ConstNullDate
                        dbValueIsNull = True
                        Return True
                    ElseIf IsDate(hostvalue) Then
                        dbvalue = CDate(hostvalue)
                        Return True
                    Else
                        Call CoreMessageHandler(subname:="XSlot.convert2DBValue",
                                              message:="OTDB data " & hostvalue & " is not convertible to Date",
                                              arg1:=hostvalue)

                        dbvalue = ConstNullDate
                        dbValueIsEmpty = True
                        Return False
                    End If

                Case otFieldDataType.Bool
                    If hostvalue Is Nothing OrElse hostValueIsNull = True Then
                        dbvalue = False
                        dbValueIsNull = True
                        Return True
                    ElseIf TypeOf (hostvalue) Is Boolean Then
                        dbvalue = hostvalue
                        Return True
                    ElseIf IsNumeric(hostvalue) Then
                        If hostvalue = 0 Then
                            dbvalue = False
                        Else
                            dbvalue = True
                        End If
                        Return True
                    ElseIf String.IsNullOrWhiteSpace(hostvalue.ToString) Then
                        dbvalue = False
                        Return True
                    ElseIf Not String.IsNullOrWhiteSpace(hostvalue.ToString) Then
                        dbvalue = True
                        Return True
                    Else
                        Call CoreMessageHandler(subname:="XSlot.convert2DBValue",
                                            message:="OTDB data " & hostvalue & " is not convertible to boolean",
                                            arg1:=hostvalue)

                        dbvalue = True
                        dbValueIsEmpty = True
                        Return False
                    End If

                Case otFieldDataType.Binary
                    dbvalue = hostvalue
                    Return True
                Case Else
                    Call CoreMessageHandler(subname:="XSlot.convert2DBValue",
                                            message:="type has no converter",
                                            arg1:=hostvalue)
                    dbvalue = Nothing
                    dbValueIsEmpty = True
                    Return False
            End Select

        End Function

    End Class
    '************************************************************************************
    '***** CLASS XEnvelope is an object to map Values from OTDB to ordinals in the Application 
    '*****       Environment
    ''' <summary>
    ''' XChange Envelope is a Member of a Bag and Contains Pairs of ordinal, XSlot
    ''' </summary>
    ''' <remarks></remarks>
    Public Class XEnvelope
        Implements IEnumerable(Of XSlot)

        Private _xbag As XBag
        Private _xchangeconfig As XConfig

        Private _IsPrechecked As Boolean = False
        Private _PrecheckedOk As Boolean = False
        Private _PrecheckTimestamp As Date = ConstNullDate

        Private _IsXChanged As Boolean = False
        Private _XChangedOK As Boolean = False
        Private _XChangedTimestamp As Date = ConstNullDate

        Private _slots As New SortedDictionary(Of Ordinal, XSlot) 'the map
        Private _msglog As New ObjectLog

        '** events for convert values
        Public Event ConvertRequest2HostValue As EventHandler(Of ConvertRequestEventArgs)
        Public Event ConvertRequestDBValue As EventHandler(Of ConvertRequestEventArgs)

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="xbag"></param>
        ''' <remarks></remarks>
        Public Sub New(xbag As XBag)
            _xbag = xbag
            _xchangeconfig = xbag.XChangeDefaultConfig
            '** add handlers
            AddHandler Me.ConvertRequest2HostValue, AddressOf xbag.OnRequestConvert2HostValue
            AddHandler Me.ConvertRequestDBValue, AddressOf xbag.OnRequestConvert2DBValue
        End Sub

#Region "Properties"
        ''' <summary>
        ''' get the prechecked flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsPrechecked As Boolean
            Get
                Return _IsPrechecked
            End Get
            Private Set(ByVal value As Boolean)
                _IsPrechecked = value
            End Set
        End Property
        ''' <summary>
        ''' gets the precheck result - only valid if IsPrechecked is true
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PrecheckedOK As Boolean
            Get
                Return _PrecheckedOk
            End Get
            Private Set(ByVal value As Boolean)
                _PrecheckedOk = value
            End Set
        End Property
        ''' <summary>
        ''' gets the timestamp for the precheck
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PrecheckTimestamp As Date
            Get
                Return _PrecheckTimestamp
            End Get
        End Property
        ''' <summary>
        ''' returns true if successfully processed (exchanged)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ProcessedOk As Boolean
            Get
                Return _XChangedOK
            End Get
        End Property
        ''' <summary>
        ''' returns true if the envelope was xchanged / processed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsProcessed As Boolean
            Get
                Return _IsXChanged
            End Get
            Set(ByVal value As Boolean)
                _IsXChanged = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the processed date.
        ''' </summary>
        ''' <value>The processed date.</value>
        Public ReadOnly Property ProcessedTimestamp() As DateTime
            Get
                Return Me._XChangedTimestamp
            End Get

        End Property

        ''' <summary>
        ''' returns the msglog associated with this xEnvelope
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MsgLog() As ObjectLog
            Get
                Return _msglog
            End Get
        End Property
        ''' <summary>
        ''' Gets the xchangeconfig.
        ''' </summary>
        ''' <value>The xchangeconfig.</value>
        Public ReadOnly Property Xchangeconfig() As XConfig
            Get
                Return Me._xchangeconfig
            End Get
        End Property
#End Region

#Region "Administrative Function"


        Public Function ordinals() As System.Collections.Generic.SortedDictionary(Of Ordinal, XSlot).KeyCollection
            Return _slots.Keys
        End Function
        '**** check functions if exists
        Public Function Containsordinal(ByVal [ordinal] As Ordinal) As Boolean
            Return _slots.ContainsKey(ordinal)
        End Function
        Public Function Containsordinal(ByVal [ordinal] As Long) As Boolean
            Return Me.Containsordinal(New Ordinal([ordinal]))
        End Function
        Public Function Containsordinal(ByVal [ordinal] As String) As Boolean
            Return Me.Containsordinal(New Ordinal([ordinal]))
        End Function
        ''' <summary>
        ''' returns true if in the XConfig a Slot is available for the Fieldname
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasConfigFieldname(ByVal fieldname As String, Optional tablename As String = "") As Boolean
            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.AddByFieldname")
                Return False
            End If

            Dim aXChangeMember = _xchangeconfig.AttributeByFieldName(fieldname:=fieldname, tablename:=tablename)

            If aXChangeMember Is Nothing Then
                Return False
            Else
                Return True
            End If
        End Function
        ''' <summary>
        ''' returns true if in the XConfig a Slot is available for the XChange ID
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasConfigID(ByVal id As String, Optional tablename As String = "") As Boolean

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.getvaluebyID")
                Return Nothing
            End If

            Dim aXChangeMember = _xchangeconfig.AttributeByID(ID:=id, objectname:=tablename)
            If aXChangeMember Is Nothing Then
                Return False
            Else
                Return True
            End If
        End Function
        ''' <summary>
        ''' remove the slot by ordinal
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RemoveSlot(ByVal key As Long) As Boolean
            Me.RemoveSlot(New Ordinal(key))
        End Function
        ''' <summary>
        ''' remove the slot by ordinal
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RemoveSlot(ByVal key As String) As Boolean
            Me.RemoveSlot(New Ordinal(key))
        End Function
        ''' <summary>
        ''' remove the slot by ordinal
        ''' </summary>
        ''' <param name="ordinal"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RemoveSlot(ByVal ordinal As Ordinal) As Boolean
            If Me.Containsordinal(ordinal) Then
                RemoveHandler _slots.Item(ordinal).ConvertRequest2DBValue, AddressOf Me.OnRequestConvert2DBValue
                RemoveHandler _slots.Item(ordinal).ConvertRequest2HostValue, AddressOf Me.OnRequestConvert2HostValue
                _slots.Remove(ordinal)
                Return True
            End If
            Return False
        End Function

        ''' <summary>
        ''' clear the Envelope from all slots
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Clear() As Boolean
            Dim aordinalList = _slots.Keys.ToList
            For Each anordinal In aordinalList
                RemoveSlot(anordinal)
            Next
            _slots.Clear()
            If _slots.Count > 0 Then Return False
            Return True
        End Function
        ''' <summary>
        ''' sets the value of an existing slot given by ordinal
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        ''' <param name="isHostValue"></param>
        ''' <param name="overwrite"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetSlotValue(ByVal key As Long, ByVal value As Object, _
                                     Optional ByVal isHostValue As Boolean = True, _
                                     Optional overwrite As Boolean = False, _
                                      Optional ValueIsNull As Boolean = False, _
                                            Optional SlotIsEmpty As Boolean = False) As Boolean
            Return Me.SetSlotValue(ordinal:=New Ordinal(key), value:=value, isHostValue:=isHostValue, overwrite:=overwrite, ValueIsNull:=ValueIsNull, SlotIsEmpty:=SlotIsEmpty)
        End Function
        ''' <summary>
        ''' sets the value of an existing slot given by ordinal
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        ''' <param name="isHostValue"></param>
        ''' <param name="overwrite"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetSlotValue(ByVal key As String, ByVal value As Object, _
                                     Optional ByVal isHostValue As Boolean = True, _
                                     Optional overwrite As Boolean = False, _
                                     Optional valueisNull As Boolean = False, _
                                     Optional SlotIsEmpty As Boolean = False) As Boolean
            Return Me.SetSlotValue(ordinal:=New Ordinal(key), value:=value, isHostValue:=isHostValue, overwrite:=overwrite, ValueIsNull:=valueisNull, SlotIsEmpty:=SlotIsEmpty)
        End Function
        ''' <summary>
        ''' set the value of an existing slot given by ordinal
        ''' </summary>
        ''' <param name="ordinal"></param>
        ''' <param name="value"></param>
        ''' <param name="isHostValue"></param>
        ''' <param name="overwrite"></param>
        ''' <returns>returns true if successfull</returns>
        ''' <remarks></remarks>
        Public Function SetSlotValue(ByVal ordinal As Ordinal, ByVal value As Object,
                                     Optional ByVal isHostValue As Boolean = True,
                                     Optional overwrite As Boolean = False, _
                                      Optional ValueIsNull As Boolean = False, _
                                     Optional SlotIsEmpty As Boolean = False) As Boolean
            ' Add slot if the ordinal is in the config
            ' take the first Attribute which has the ordinal
            If Not Me.Containsordinal(ordinal) Then
                Dim MemberList = Me.Xchangeconfig.AttributesByordinal(ordinal:=ordinal)
                Dim anAttribute As XConfigAttributeEntry = Nothing
                For Each aMember In MemberList
                    If aMember.IsAttributeEntry Then
                        anAttribute = TryCast(aMember, XConfigAttributeEntry)
                        If anAttribute IsNot Nothing Then
                            Exit For
                        End If
                    End If
                Next
                If anAttribute IsNot Nothing Then
                    Me.AddSlot(slot:=New XSlot(xenvelope:=Me, attribute:=anAttribute, hostvalue:=Nothing, isEmpty:=True))
                    overwrite = True
                End If
            End If
            ' try again
            If Me.Containsordinal(ordinal) Then
                Dim aSlot = _slots.Item(key:=ordinal)
                '* reset the value if meant to be empty
                If SlotIsEmpty Then
                    value = Nothing
                End If
                If aSlot.IsEmpty Or aSlot.IsNull Or overwrite Then
                    If isHostValue Then
                        aSlot.HostValue = value
                    Else
                        aSlot.DBValue = value
                    End If
                    aSlot.IsEmpty = SlotIsEmpty
                    aSlot.IsNull = ValueIsNull
                End If

            End If

        End Function

        ''' <summary>
        ''' returns a Slot by ordinal
        ''' </summary>
        ''' <param name="ordinal"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSlot(ByRef ordinal As Ordinal) As XSlot
            If Me.Containsordinal(ordinal) Then
                'TODO: Differentiate if the value is coming from which object -> donot overwrite with wrong information
                Return _slots.Item(key:=ordinal)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns a List of Slot of a certain ObjectName
        ''' </summary>
        ''' <param name="ordinal"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSlotByObject(ByRef objectname As String) As List(Of XSlot)
            Dim aList As New List(Of XSlot)

            If Me.Xchangeconfig Is Nothing Then
                Return aList
            End If
            For Each anAttribute In Me.Xchangeconfig.AttributesByObjectName(objectname:=objectname)
                If Me.HasSlotByFieldname(fieldname:=anAttribute.Entryname, tablename:=objectname) Then
                    aList.Add(Me.GetSlot(ordinal:=anAttribute.ordinal))
                End If
            Next
            Return aList
        End Function
        ''' <summary>
        ''' Add a Slot by ordinal
        ''' </summary>
        ''' <param name="slot"></param>
        ''' <param name="replaceSlotIfExists"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddSlot(ByRef slot As XSlot, Optional replaceSlotIfExists As Boolean = False) As Boolean
            If Me.Containsordinal(slot.ordinal) Then
                'TODO: Differentiate if the value is coming from which object -> donot overwrite with wrong information
                If replaceSlotIfExists Then
                    Me.RemoveSlot(slot.ordinal)
                Else
                    Return False
                End If
            End If

            'add our EventHandler for ConvertRequests -> done in new of Slot
            'AddHandler slot.ConvertRequest2HostValue, AddressOf Me.OnRequestConvert2HostValue
            'AddHandler slot.ConvertRequest2DBValue, AddressOf Me.OnRequestConvert2DBValue
            ' add the slot
            _slots.Add(slot.ordinal, value:=slot)
            Return True
        End Function
        '*****
        ''' <summary>
        ''' set a slot by ID Reference. get the ordinal from the id and set the value
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="value"></param>
        ''' <param name="isHostValue"></param>
        ''' <param name="tablename"></param>
        ''' <param name="replaceSlotIfExists"></param>
        '''  <param name="extendXConfig"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddSlotByID(ByVal id As String, ByVal value As Object,
                                    Optional ByVal isHostValue As Boolean = True,
                                    Optional tablename As String = "",
                                    Optional replaceSlotIfExists As Boolean = False,
                                    Optional extendXConfig As Boolean = False, _
                                    Optional ValueIsNull As Boolean = False, _
                                            Optional SlotIsEmpty As Boolean = False, _
                                             Optional isXchanged As Boolean = True, _
                                            Optional isReadonly As Boolean = False, _
                                            Optional xcmd As otXChangeCommandType = Nothing) As Boolean

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.AddByID")
                Return False
            End If

            Dim aXChangeMember = _xchangeconfig.AttributeByID(ID:=id, objectname:=tablename)
            Return Me.AddSlotbyAttribute(configmember:=aXChangeMember, value:=value, isHostValue:=isHostValue, SlotIsEmpty:=SlotIsEmpty, ValueIsNull:=ValueIsNull, _
                                      replaceSlotIfexists:=replaceSlotIfExists)

            If aXChangeMember Is Nothing And extendXConfig Then
                _xchangeconfig.AddAttributeByID(id:=id, objectname:=tablename, [readonly]:=isReadonly, isXChanged:=isXchanged, xcmd:=xcmd)
                aXChangeMember = _xchangeconfig.AttributeByID(ID:=id, objectname:=tablename)
            End If

            If aXChangeMember IsNot Nothing Then
                Return Me.AddSlotbyAttribute(configmember:=aXChangeMember, value:=value, isHostValue:=isHostValue, SlotIsEmpty:=SlotIsEmpty, ValueIsNull:=ValueIsNull, _
                                      replaceSlotIfexists:=replaceSlotIfExists)
            Else
                Return False
            End If
        End Function
        ''' <summary>
        ''' Add a Slot by fieldname
        ''' </summary>
        ''' <param name="fieldname"></param>
        ''' <param name="value"></param>
        ''' <param name="isHostValue"></param>
        ''' <param name="tablename"></param>
        ''' <param name="overwriteValue"></param>
        ''' <param name="extendXConfig"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddSlotByFieldname(ByVal fieldname As String, ByVal value As Object,
                                           Optional ByVal isHostValue As Boolean = True,
                                            Optional tablename As String = "",
                                            Optional overwriteValue As Boolean = False,
                                            Optional extendXConfig As Boolean = False, _
                                            Optional ValueIsNull As Boolean = False, _
                                            Optional SlotIsEmpty As Boolean = False, _
                                            Optional isXchanged As Boolean = True, _
                                            Optional isReadonly As Boolean = False, _
                                            Optional xcmd As otXChangeCommandType = Nothing) As Boolean

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.AddByFieldname")
                Return False
            End If

            Dim aXChangeMember = _xchangeconfig.AttributeByFieldName(fieldname:=fieldname, tablename:=tablename)
            If aXChangeMember Is Nothing And extendXConfig Then
                _xchangeconfig.AddAttributeByField(entryname:=fieldname, objectname:=tablename, isXChanged:=isXchanged, [readonly]:=isReadonly, _
                                                   xcmd:=xcmd)
                aXChangeMember = _xchangeconfig.AttributeByFieldName(fieldname:=fieldname, tablename:=tablename)
            End If

            If aXChangeMember IsNot Nothing Then
                Return Me.AddSlotbyAttribute(configmember:=aXChangeMember, value:=value, isHostValue:=isHostValue, overwriteValue:=overwriteValue, _
                                             ValueIsNull:=ValueIsNull, SlotIsEmpty:=SlotIsEmpty)
            Else
                Return False
            End If

        End Function
        ''' <summary>
        ''' Add a slot by a configMember definition
        ''' </summary>
        ''' <param name="configmember"></param>
        ''' <param name="value"></param>
        ''' <param name="isHostValue"></param>
        ''' <param name="tablename"></param>
        ''' <param name="overwriteValue"></param>
        ''' <param name="removeSlotIfExists"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddSlotbyAttribute(ByRef configmember As iConfigMember, ByVal value As Object,
                                        Optional ByVal isHostValue As Boolean = True,
                                        Optional tablename As String = "",
                                        Optional overwriteValue As Boolean = False,
                                        Optional replaceSlotIfexists As Boolean = False, _
                                         Optional ValueIsNull As Boolean = False, _
                                            Optional SlotIsEmpty As Boolean = False) As Boolean

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.AddByMember")
                Return False
            End If

            If Not configmember Is Nothing AndAlso (configmember.IsLoaded Or configmember.IsCreated) Then
                If Me.Containsordinal(ordinal:=configmember.ordinal) And Not replaceSlotIfexists Then
                    If overwriteValue Then
                        Dim aSlot As XSlot = _slots.Item(key:=configmember.ordinal)
                        If isHostValue Then
                            aSlot.HostValue = value
                        Else
                            aSlot.DBValue = value
                        End If
                        aSlot.IsEmpty = SlotIsEmpty
                        aSlot.IsNull = ValueIsNull
                        Return True
                    End If
                    Return False
                Else
                    Dim aNewSlot As XSlot = New XSlot(Me, attribute:=configmember)
                    If isHostValue Then
                        aNewSlot.HostValue = value

                    Else
                        aNewSlot.DBValue = value
                    End If
                    aNewSlot.IsEmpty = SlotIsEmpty
                    aNewSlot.IsNull = ValueIsNull
                    Return Me.AddSlot(slot:=aNewSlot, replaceSlotIfExists:=replaceSlotIfexists)
                End If
            End If
        End Function
        ''' <summary>
        ''' returns the Slot's value by ID
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="tablename"></param>
        ''' <param name="asHostValue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSlotValueByID(ByVal id As String, Optional tablename As String = "", Optional asHostValue As Boolean = True) As Object

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.getvaluebyID")
                Return Nothing
            End If

            Dim aXChangeMember = _xchangeconfig.AttributeByID(ID:=id, objectname:=tablename)
            If aXChangeMember IsNot Nothing Then
                Return Me.GetSlotValueByAttribute(aXChangeMember)
            Else
                CoreMessageHandler(message:="XChangeConfig '" & Me.Xchangeconfig.Configname & "' does not include the id", arg1:=id, messagetype:=otCoreMessageType.ApplicationWarning, subname:="XEnvelope.GetSlotValueByID")
                Return Nothing
            End If

        End Function
        ''' <summary>
        ''' return true if there is a slot by ID
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasSlotByID(ByVal id As String, Optional tablename As String = "") As Object

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.HasSlotByID")
                Return Nothing
            End If

            Dim aXChangeMember = _xchangeconfig.AttributeByID(ID:=id, objectname:=tablename)
            If aXChangeMember IsNot Nothing Then
                Return Me.HasSlotByAttribute(aXChangeMember)
            Else
                Return False
            End If

        End Function
        ''' <summary>
        ''' returns the slot's value by fieldname
        ''' </summary>
        ''' <param name="fieldname"></param>
        ''' <param name="tablename"></param>
        ''' <param name="asHostValue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSlotValueByFieldname(ByVal fieldname As String, Optional tablename As String = "", Optional asHostValue As Boolean = True) As Object

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.GetValueByFieldname")
                Return Nothing
            End If

            Dim aXChangeMember As XConfigAttributeEntry = _xchangeconfig.AttributeByFieldName(fieldname:=fieldname, tablename:=tablename)
            If aXChangeMember IsNot Nothing Then
                Return Me.GetSlotValueByAttribute(aXChangeMember)
            Else
                CoreMessageHandler(message:="xconfiguration '" & Me.Xchangeconfig.Configname & "' does not include fieldname", entryname:=fieldname, tablename:=tablename, _
                                   messagetype:=otCoreMessageType.ApplicationWarning, subname:="Xenvelope.GetSlotValueByFieldname")
                Return Nothing
            End If

        End Function
        ''' <summary>
        ''' returns true if there is a slot by fieldname
        ''' </summary>
        ''' <param name="fieldname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasSlotByFieldname(ByVal fieldname As String, Optional tablename As String = "") As Boolean

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.HasSlotByFieldname")
                Return Nothing
            End If
            Dim aXChangeMember As XConfigAttributeEntry = _xchangeconfig.AttributeByFieldName(fieldname:=fieldname, tablename:=tablename)
            If aXChangeMember IsNot Nothing Then
                Return Me.HasSlotByAttribute(aXChangeMember)
            Else
                Return False

            End If

        End Function

        ''' <summary>
        ''' returns the slot's value by attribute
        ''' </summary>
        ''' <param name="xchangemember"></param>
        ''' <param name="asHostValue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSlotValueByAttribute(ByRef xchangemember As XConfigAttributeEntry, Optional asHostValue As Boolean = True) As Object

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.GetSlotValueByAttribute")
                Return Nothing
            End If

            If Not xchangemember Is Nothing AndAlso (xchangemember.IsLoaded Or xchangemember.IsCreated) Then
                Return Me.GetSlotValue(ordinal:=New Ordinal(xchangemember.ordinal), asHostvalue:=asHostValue)
            Else
                Call CoreMessageHandler(message:="XChangeConfigMember is nothing", messagetype:=otCoreMessageType.InternalWarning, subname:="XEnvelope.GetValueByMember")
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns True if there is a slot by XConfig Member by XChangemember
        ''' </summary>
        ''' <param name="xchangemember"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasSlotByAttribute(ByRef xchangemember As XConfigAttributeEntry) As Boolean

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.HasSlotByAttribute")
                Return False
            End If

            If xchangemember IsNot Nothing AndAlso (xchangemember.IsLoaded Or xchangemember.IsCreated) Then
                If _slots.ContainsKey(key:=xchangemember.ordinal) Then
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If
        End Function
        ''' <summary>
        ''' returns the Attribute of a slot by fieldname and tablename
        ''' </summary>
        ''' <param name="fieldname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetAttributeByFieldname(ByVal fieldname As String, Optional tablename As String = "") As XConfigAttributeEntry

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.GetAttributeByFieldname")
                Return Nothing
            End If

            Return _xchangeconfig.AttributeByFieldName(fieldname:=fieldname, tablename:=tablename)
        End Function
        ''' <summary>
        ''' returns the Attribute of a slot by id and tablename
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetAttributeByID(ByVal ID As String, Optional tablename As String = "") As XConfigAttributeEntry

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.GetAttributeByID")
                Return Nothing
            End If

            Return _xchangeconfig.AttributeByID(ID:=ID, objectname:=tablename)
        End Function

        ''' <summary>
        ''' returns the Slotsvalue as hostvalue or dbvalue by ordinal
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="asHostvalue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSlotValue(ByVal key As Long, Optional ByVal asHostValue As Boolean = False) As Object
            Return Me.GetSlotValue(ordinal:=New Ordinal(key), asHostvalue:=asHostValue)
        End Function
        ''' <summary>
        ''' returns the Slotsvalue as hostvalue or dbvalue by ordinal
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="asHostvalue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSlotValue(ByVal key As String, Optional ByVal asHostValue As Boolean = False) As Object
            Return Me.GetSlotValue(ordinal:=New Ordinal(key), asHostvalue:=asHostValue)
        End Function
        ''' <summary>
        ''' returns the Slotsvalue as hostvalue or dbvalue by ordinal
        ''' </summary>
        ''' <param name="ordinal"></param>
        ''' <param name="asHostvalue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSlotValue(ByVal ordinal As Ordinal, Optional asHostvalue As Boolean = True) As Object
            If _slots.ContainsKey(key:=ordinal) Then
                Dim aSlot = _slots.Item(key:=ordinal)
                If asHostvalue Then
                    Return aSlot.HostValue
                Else
                    Return aSlot.DBValue
                End If
            Else
                Return Nothing
            End If
        End Function
        '*** enumerators -> get values
        Public Function GetEnumerator() As IEnumerator(Of XSlot) Implements IEnumerable(Of XSlot).GetEnumerator
            Return _slots.Values.GetEnumerator
        End Function
        Public Function GetEnumerator1() As Collections.IEnumerator Implements Collections.IEnumerable.GetEnumerator
            Return _slots.Values.GetEnumerator
        End Function
#End Region

        ''' <summary>
        ''' Eventhandler for the Slots OnRequestConvert2Hostvalue
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnRequestConvert2HostValue(ByVal sender As Object, ByVal e As ConvertRequestEventArgs)
            RaiseEvent ConvertRequest2HostValue(sender, e) ' cascade
        End Sub
        ''' <summary>
        ''' EventHandler for the Slots OnRequestConvert2DBValue
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnRequestConvert2DBValue(ByVal sender As Object, ByVal e As ConvertRequestEventArgs)
            RaiseEvent ConvertRequestDBValue(sender, e) ' cascade
        End Sub

        ''' <summary>
        ''' returns the Object XCommand
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectXCmd(ByVal objectname As String) As otXChangeCommandType
            Dim anObject As XConfigObjectEntry = Me.Xchangeconfig.ObjectByName(objectname:=objectname)
            If anObject IsNot Nothing Then
                Return anObject.XChangeCmd
            Else
                Return 0
            End If
        End Function
        ''' <summary>
        ''' run XChange Precheck on the Envelope
        ''' </summary>
        ''' <param name="aMapping"></param>
        ''' <param name="MSGLOG"></param>
        ''' <param name="SUSPENDOVERLOAD"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunXPreCheck(Optional ByRef msglog As ObjectLog = Nothing,
                                     Optional ByVal suspendoverload As Boolean = True) As Boolean
            Dim flag As Boolean

            ' set msglog
            If msglog Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                msglog = _msglog
                'msglog.Create()
            End If

            ' suspend Overloading
            If suspendoverload Then Call SuspendOverloading(True)


            '* go through each object
            For Each anObject In _xbag.XChangeDefaultConfig.ObjectsByOrderNo

                ' special handling for special objects
                Select Case LCase(anObject.Objectname)

                    ' currtargets
                    Case "tblcurrtargets"
                        flag = True

                        ' currschedules
                    Case "tblcurrschedules"
                        flag = True

                        ' schedules
                    Case "tblschedules"
                        flag = True

                        ' HACK: CARTYPES
                    Case "tblconfigs"
                        flag = True

                        ' Targets
                    Case "tbldeliverabletargets"
                        'flag = clsOTDBDeliverableTarget.runXPreCheck(Me, msglog)
                        '
                    Case Else
                        ' default
                        'flag = Me.runDefaultXPreCheck(Me, msglog)
                End Select
            Next

            ' suspend Overloading
            If suspendoverload Then Call SuspendOverloading(False)

            _PrecheckTimestamp = Date.Now
            _IsPrechecked = True
            _PrecheckedOk = flag
            Return _PrecheckedOk
        End Function

        ''' <summary>
        ''' run XChange for this Envelope
        ''' </summary>
        ''' <param name="msglog"></param>
        ''' <param name="suspendoverload"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunXChange(Optional ByRef msglog As ObjectLog = Nothing,
                                   Optional ByVal suspendoverload As Boolean = True) As Boolean
            Dim flag As Boolean
            Dim aTarget As New Target
            Dim aSchedule As New Schedule
            Dim aDeliverable As New Deliverable

            ' set msglog
            If msglog Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                msglog = _msglog
                'msglog.Create(Me.msglogtag)
            End If

            ' suspend Overloading
            If suspendoverload Then Call SuspendOverloading(True)

            If _XChangedTimestamp = ConstNullDate Then
                _XChangedTimestamp = Date.Now
            End If

            '* go through each object
            For Each anConfigObject As XConfigObjectEntry In Me.Xchangeconfig.ObjectsByOrderNo

                ' special handling for special objects
                Select Case LCase(anConfigObject.Objectname)

                    ' currschedules
                    Case LCase(CurrentSchedule.ConstTableID)
                        flag = True

                    Case LCase(XOutline.constTableID)
                        flag = True

                        ' Tracks
                    Case LCase(Track.constTableID)
                        flag = True

                        ' HACK: CARTYPES
                    Case "tblconfigs"
                        'flag = flag And aDeliverable.runCartypesXChange(Me, msglog)
                        flag = True

                        ' Targets
                    Case LCase(Target.constTableID)
                        'flag = flag And aTarget.runXChange(Me, msglog)
                        flag = True
                End Select

                '****
                '**** Standards
                If Not flag Then
                    '** check through reflection
                    Dim anObjectType As System.Type = ot.GetDataObjectType(anConfigObject.Objectname)
                    If anObjectType IsNot Nothing AndAlso _
                        anObjectType.GetInterface(GetType(iotXChangeable).FullName) IsNot Nothing Then

                        Dim aXChangeable As iotXChangeable = Activator.CreateInstance(anObjectType)
                        flag = flag And aXChangeable.RunXChange(Me)
                    Else
                        ' default
                        flag = flag And RunDefaultXchange(anConfigObject, msglog)
                    End If
                End If
              

            Next

            ' suspend Overloading
            If suspendoverload Then Call SuspendOverloading(False)

            _IsXChanged = True
            _PrecheckedOk = flag
            Return True
        End Function

        ''' <summary>
        ''' create and update a object 
        ''' </summary>
        ''' <param name="xobject"></param>
        ''' <param name="record"></param>
        ''' <param name="pkarray"></param>
        ''' <param name="msglog"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function CreateandUpdateObject(ByRef xobject As XConfigObjectEntry,
                                               ByRef record As ormRecord,
                                               ByRef pkarray() As Object,
                                               Optional ByRef msglog As ObjectLog = Nothing
                                                ) As Boolean
            Dim aDataObjectType As System.Type
            Dim aDataobject As iormPersistable
            Dim aDataInfusable As iormInfusable
            Dim aValue As Object = Nothing
            Dim anOldValue As Object = Nothing
            Dim aSlot As XSlot

            Dim persistflag As Boolean = False

            '** BETTER TO CREATE A NEW OBJECT -> Default values
            If ot.GetDataObjectType(tableid:=xobject.Objectname) IsNot Nothing Then
                aDataObjectType = GetDataObjectType(tableid:=xobject.Objectname)
                aDataobject = Activator.CreateInstance(aDataObjectType)
                aDataInfusable = aDataobject
            Else
                aDataobject = Nothing
                aDataInfusable = Nothing
            End If

            '** create new object
            '**
            If xobject.XChangeCmd = otXChangeCommandType.Update And record Is Nothing Then
                Return False
            ElseIf xobject.XChangeCmd = otXChangeCommandType.UpdateCreate And record Is Nothing Then
                '** RECORD based Object creation
                record = New ormRecord
                record.SetTable(xobject.Objectname, fillDefaultValues:=True)

                '** BETTER TO CREATE A NEW OBJECT -> Default values
                If aDataobject IsNot Nothing Then
                    If Not aDataobject.Create(pkArray:=pkarray) Then
                        CoreMessageHandler(message:="Data object with same primary keys exists", messagetype:=otCoreMessageType.ApplicationError, subname:="XEnvelope.RunDefaultXChange4Object")
                        aDataobject.LoadBy(pkArray:=pkarray)
                    End If
                End If
                '** set to updateCreate
                For Each anAttribute In Me.Xchangeconfig.AttributesByObjectName(objectname:=xobject.Objectname)
                    anAttribute.XChangeCmd = otXChangeCommandType.UpdateCreate
                    anAttribute.IsXChanged = True
                Next
            End If

            '*** set values of object
            '***
            For Each anAttribute In Me.Xchangeconfig.AttributesByObjectName(objectname:=xobject.Objectname)
                If anAttribute.IsXChanged AndAlso Not anAttribute.IsReadOnly Then
                    If (anAttribute.XChangeCmd = otXChangeCommandType.Update Or anAttribute.XChangeCmd = otXChangeCommandType.UpdateCreate) Then
                        aSlot = Me.GetSlot(ordinal:=anAttribute.ordinal)
                        If aSlot IsNot Nothing AndAlso Not aSlot.IsEmpty Then
                            '* get Value from Slot
                            aValue = aSlot.DBValue
                            '* get old value
                            If record.HasIndex(anIndex:=anAttribute.Entryname) Then
                                anOldValue = record.GetValue(index:=anAttribute.Entryname)
                            Else
                                anOldValue = Nothing
                            End If
                            '** change if different and not empty
                            If aValue <> anOldValue Then
                                record.SetValue(index:=anAttribute.Entryname, value:=aValue)
                                persistflag = True
                            End If
                        End If
                    End If
                End If
            Next

            '' if a new record has not all fields set -> ?! what to do then ?

            '** BETTER TO CREATE A NEW OBJECT -> Default values
            If persistflag Then
                If aDataobject IsNot Nothing Then
                    aDataInfusable.Infuse(record)
                    Return aDataobject.Persist()
                Else
                    Return record.Persist
                End If
            End If

        End Function

        ''' <summary>
        ''' Run the default xchange for a given record
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="dataobject"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunDefaultXchange(ByRef record As ormRecord, _
                                          Optional xobject As XConfigObjectEntry = Nothing, _
                                          Optional pkArray As Object() = Nothing, _
                                          Optional ByRef msglog As ObjectLog = Nothing,
                                          Optional ByVal nocompounds As Boolean = False) As Boolean
            Dim aValue As Object


            '* get the config
            If xobject Is Nothing Then
                xobject = Me.Xchangeconfig.ObjectByName(objectname:=record.TableID)
            End If

            ' set msglog
            If msglog Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                msglog = _msglog
                'msglog.Create(Me.msglogtag)
            End If

            '** no record is given
            If record IsNot Nothing Then
                '*** load the record fields not the compounds !
                '***
                For Each aFieldname In record.Keys
                    Dim anObjectEntry = CurrentSession.Objects.GetEntry(entryname:=aFieldname, objectname:=xobject.Objectname)

                    If Me.HasConfigFieldname(fieldname:=aFieldname, tablename:=xobject.Objectname) AndAlso Not anObjectEntry.IsCompound Then
                        '* get the value and add it -> will be replaced as well !
                        aValue = record.GetValue(aFieldname)
                        If aValue IsNot Nothing Then
                            Me.AddSlotByFieldname(fieldname:=aFieldname, tablename:=xobject.Objectname, value:=aValue, isHostValue:=False,
                                                  overwriteValue:=False, extendXConfig:=False)
                        End If
                    End If
                Next

                '*** load the compounds
                '***
                If Not nocompounds Then
                    Dim objectType As System.Type = ot.GetDataObjectType(tableid:=xobject.Objectname)
                    If objectType IsNot Nothing AndAlso objectType.GetInterface(GetType(iotHasCompounds).FullName) IsNot Nothing Then
                        Dim aHasCompounds As iotHasCompounds = Activator.CreateInstance(objectType)
                        Dim aInfusable As iormInfusable = TryCast(aHasCompounds, iormInfusable)
                        If aInfusable IsNot Nothing Then
                            aInfusable.Infuse(record)
                            If aHasCompounds IsNot Nothing Then
                                aHasCompounds.AddSlotCompounds(Me)
                            Else
                                CoreMessageHandler(message:="the object of type " & xobject.Objectname & " cannot be casted to hasCompunds", _
                                                   subname:="XEnvelope.RunDefaultxChange", messagetype:=otCoreMessageType.InternalError)
                            End If
                        Else
                            CoreMessageHandler(message:="the object of type " & xobject.Objectname & " cannot be infused", _
                                                   subname:="XEnvelope.RunDefaultxChange", messagetype:=otCoreMessageType.InternalError)

                        End If

                    End If
                End If

            End If

            '*** run the command
            '***
            Select Case xobject.XChangeCmd


                '*** delete
                '***
                Case otXChangeCommandType.Delete

                    '**** add or update
                    '****
                Case otXChangeCommandType.Update, otXChangeCommandType.UpdateCreate
                    '* if no primary keys then refill it with the object definition from the record
                    If pkArray Is Nothing Then
                        ReDim pkArray(xobject.ObjectDefinition.GetNoPrimaryKeys)
                        '**** fill the primary key structure
                        Dim i As UShort = 0
                        For Each aPKEntry In xobject.ObjectDefinition.GetPrimaryKeyEntries
                            aValue = record.GetValue(index:=aPKEntry.Entryname)
                            If aValue Is Nothing Then
                                '* try to load from Envelope if not in record
                                aValue = Me.GetSlotValueByFieldname(fieldname:=aPKEntry.Entryname, tablename:=xobject.Objectname, asHostValue:=False)
                                If aValue IsNot Nothing Then
                                    record.SetValue(index:=aPKEntry.Entryname, value:=aValue) ' set it also in the record
                                End If
                            End If
                            If aValue IsNot Nothing Then
                                '** convert from DB to Host
                                pkArray(i) = aValue
                                i += 1
                            Else
                                Call CoreMessageHandler(message:="value of primary key is not in configuration or envelope :" & xobject.Configname,
                                                 arg1:=xobject.Objectname, entryname:=aPKEntry.Entryname, messagetype:=otCoreMessageType.ApplicationError,
                                                 subname:="XEnvelope.runDefaultXChange(Record)")
                                Return False
                            End If

                        Next
                    End If
                    '** create and Update the object
                    Return Me.CreateandUpdateObject(xobject:=xobject, record:=record, pkarray:=pkArray)

                    '*** duplicate
                    '***
                Case otXChangeCommandType.Duplicate

                    '***
                    '*** just read and return
                Case otXChangeCommandType.Read
                    Return Not record Is Nothing

                    '**** no command ?!
                Case Else
                    Call CoreMessageHandler(message:="XChangeCmd for this object is not known :" & xobject.Objectname,
                                      arg1:=xobject.XChangeCmd, tablename:=xobject.Objectname, messagetype:=otCoreMessageType.ApplicationError,
                                      subname:="XEnvelope.runDefaultXChange(Record)")
                    Return False
            End Select



        End Function
        ''' <summary>
        ''' Run the default xchange for a given object
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="dataobject"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunDefaultXchange(Of T As {iormInfusable, iormPersistable, New})(ByRef dataobject As T, _
                                                                                         Optional ByRef msglog As ObjectLog = Nothing, _
                                                                                         Optional ByVal nocompounds As Boolean = False) As Boolean
            Dim result As Boolean
            ' set msglog
            If msglog Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                msglog = _msglog
                'msglog.Create(Me.msglogtag)
            End If

            If Not dataobject.IsLoaded And Not dataobject.IsCreated Then
                CoreMessageHandler(message:="data object needs to be loaded or created", tablename:=dataobject.TableID, subname:="XEnvelope.RunDefaultXCHange(dataobject)", messagetype:=otCoreMessageType.ApplicationError)
                Return False
            End If

            Dim aXObject As XConfigObjectEntry = Me.Xchangeconfig.ObjectByName(objectname:=dataobject.TableID)
            If aXObject IsNot Nothing Then
                result = Me.RunDefaultXchange(xobject:=aXObject, record:=dataobject.Record, nocompounds:=nocompounds, msglog:=msglog)
                dataobject.Infuse(dataobject.Record) ' reinfuse
                Return result
            Else
                CoreMessageHandler(message:="dataobject's objectname is not in the xconfiguration " & Me.Xchangeconfig.Configname, tablename:=dataobject.TableID, subname:="xEnvelope.runDefaultXChange(Dataobject)", messagetype:=otCoreMessageType.ApplicationWarning)
                Return False
            End If

        End Function
        ''' <summary>
        ''' Run the Default XChange for an object by primary keys
        ''' </summary>
        ''' <param name="xobject"></param>
        ''' <param name="msglog"></param>
        ''' <param name="nocompounds"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunDefaultXChange(ByRef xobject As XConfigObjectEntry,
                                                 Optional ByRef msglog As ObjectLog = Nothing,
                                                 Optional ByVal nocompounds As Boolean = False) As Boolean
            Dim aStore As iormDataStore
            Dim aRecord As ormRecord
            Dim pkarry() As Object
            Dim aValue As Object

            ' set msglog
            If msglog Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                msglog = _msglog
                'msglog.Create(Me.msglogtag)
            End If

            '*** build the primary key array
            If xobject.ObjectDefinition.GetNoPrimaryKeys = 0 Then
                Call CoreMessageHandler(message:="primary key of table is Nothing in xchange config:" & xobject.Configname,
                                      arg1:=xobject.Objectname, messagetype:=otCoreMessageType.InternalError, subname:="XEnvelope.runDefaultXChange4Object")
                Return False
            Else
                ReDim pkarry(xobject.ObjectDefinition.GetNoPrimaryKeys)
            End If

            '**** fill the primary key structure
            Dim i As UShort = 0
            For Each aPKEntry In xobject.ObjectDefinition.GetPrimaryKeyEntries
                aValue = Me.GetSlotValueByFieldname(fieldname:=aPKEntry.Entryname, tablename:=aPKEntry.Objectname, asHostValue:=False)
                If aValue IsNot Nothing Then
                    '** convert from DB to Host
                    pkarry(i) = aValue
                    i += 1
                Else
                    Call CoreMessageHandler(message:="value of primary key is not in configuration or envelope :" & xobject.Configname,
                                     arg1:=xobject.Objectname, entryname:=aPKEntry.Entryname, messagetype:=otCoreMessageType.ApplicationError,
                                     subname:="XEnvelope.runDefaultXChange4Object")
                    Return False
                End If

            Next

            '*** read the data
            '***
            aStore = GetTableStore(xobject.Objectname)
            '** read data from store
            aRecord = aStore.GetRecordByPrimaryKey(pkarry)

            '** run it with the record
            Return Me.RunDefaultXchange(record:=aRecord, xobject:=xobject, msglog:=msglog, nocompounds:=nocompounds)

        End Function

        '***** fillMappingWithCompounds
        '*****
        Private Function fillMappingWithCompounds(ByRef RECORD As ormRecord, ByRef MAPPING As Dictionary(Of Object, Object),
                                                  ByRef ORIGMAPPING As Dictionary(Of Object, Object),
        ByRef TABLE As ObjectDefinition,
        Optional ByRef MSGLOG As ObjectLog = Nothing) As Boolean
            Dim aConfigmember As clsOTDBXChangeMember
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim aCompRecordColl As New List(Of ormRecord)
            Dim aCompTableDir As New Dictionary(Of String, Dictionary(Of String, ObjectEntryDefinition))
            Dim compoundKeys As Object
            Dim aCompField As Object
            Dim aCompValue As Object
            Dim tablename As Object
            Dim aTableEntry As ObjectEntryDefinition
            Dim anEntryDir As New Dictionary(Of String, ObjectEntryDefinition)
            Dim aKey As String
            Dim pkarry() As Object
            Dim i As Integer
            Dim m As Object
            Dim aTableName As String
            Dim compValueFieldName As String
            Dim compIDFieldname As String
            Dim aVAlue As Object

            Dim aSchedule As New Schedule
            Dim aScheduleMilestone As New ScheduleMilestone
            Dim specialHandling As Boolean

            ' store each Compound
            For Each m In TABLE.Entries
                aTableEntry = m
                If aTableEntry.ID <> "" And aTableEntry.IsCompound Then
                    If aCompTableDir.ContainsKey(key:=aTableEntry.CompoundTablename) Then
                        anEntryDir = aCompTableDir.Item(key:=aTableEntry.CompoundTablename)
                    Else
                        anEntryDir = New Dictionary(Of String, ObjectEntryDefinition)
                        Call aCompTableDir.Add(key:=aTableEntry.CompoundTablename, value:=anEntryDir)
                    End If
                    ' add the Entry
                    If Not anEntryDir.ContainsKey(key:=aTableEntry.ID) Then
                        Call anEntryDir.Add(key:=UCase(aTableEntry.ID), value:=aTableEntry)
                    Else
                        Assert(False)

                    End If
                End If
            Next m

            '**********************************************************
            '**** SPECIAL HANDLING OF tblschedules -> Milestones
            '**********************************************************
            If LCase(TABLE.Name) = LCase(aSchedule.TableID) Then
                Dim anUID As Long
                Dim anUPDC As Long

                If Not IsNull(RECORD.GetValue("uid")) Then
                    anUID = CLng(RECORD.GetValue("uid"))
                Else
                    anUID = 0
                End If
                If Not IsNull(RECORD.GetValue("updc")) Then
                    anUPDC = CLng(RECORD.GetValue("updc"))
                Else
                    anUPDC = 0
                End If
                ' found
                If anUPDC <> 0 And anUID <> 0 Then
                    If aSchedule.Loadby(UID:=anUID, updc:=anUPDC) Then
                        specialHandling = True
                    Else
                        specialHandling = False
                    End If
                Else
                    specialHandling = False
                    'Debug.Print("mmh no schedule for ", anUID, anUPDC)
                End If
            Else
                specialHandling = False
            End If

            '*** for each compound table
            '***
            For Each tablename In aCompTableDir.Keys

                ' get the Entries
                aTableName = CStr(tablename)
                anEntryDir = aCompTableDir.Item(key:=aTableName)
                aTableEntry = anEntryDir.First.Value      'first item
                compIDFieldname = aTableEntry.CompoundIDFieldname
                compValueFieldName = aTableEntry.CompoundValueFieldname

                ' look up the keys
                compoundKeys = aTableEntry.CompoundRelation
                If Not IsArrayInitialized(compoundKeys) Then
                    Call CoreMessageHandler(arg1:=aTableEntry.Name, message:="no compound relation found for fieldname", subname:="clsOTDBXchangeConfig.fillMappingWithCompounds")
                    fillMappingWithCompounds = False
                    Exit Function
                End If
                ReDim pkarry(UBound(compoundKeys))
                For i = LBound(compoundKeys) To UBound(compoundKeys)
                    pkarry(i) = RECORD.GetValue(compoundKeys(i))
                Next i


                '**********************************************************
                '**** SPECIAL HANDLING OF tblschedules -> Milestones
                '**********************************************************
                If LCase(aTableName) = LCase(ScheduleMilestone.constTableID) And specialHandling Then

                    For Each aTableEntry In TABLE.Entries
                        'aTableEntry = m
                        If aTableEntry.ID <> "" And aTableEntry.IsCompound Then
                            aCompValue = aSchedule.GetMilestoneValue(ID:=aTableEntry.ID)
                            'Set aTableEntry = anEntryDir.Item(Key:=LCase(aCompField)) -> should be the same
                            'aConfigmember = Me.AttributeByFieldName(fieldname:=aTableEntry.ID, tablename:=aTableEntry.Objectname)
                            aVAlue = Nothing
                            ' map it back -> set values which are not set (e.g. other keys)
                            If Not aConfigmember Is Nothing Then
                                ' save old value
                                If ORIGMAPPING.ContainsKey(key:=aConfigmember.ordinal.Value) Then
                                    Call ORIGMAPPING.Remove(key:=aConfigmember.ordinal.Value)
                                End If
                                Call aConfigmember.convertValue4DB(aCompValue, aVAlue)    '-> MAPPING SHOULD BE HOST DATA
                                Call ORIGMAPPING.Add(key:=aConfigmember.ordinal.Value, value:=aVAlue)

                                ' overload depending otRead and not PrimaryKey or otUpdate
                                ' run the original DB Value (runXCHange makes s 4DB too)
                                Call aConfigmember.runXChange(MAPPING:=MAPPING, VARIABLE:=aCompValue, MSGLOG:=MSGLOG)

                            End If
                        End If
                    Next

                Else
                    '*************************************************************
                    '***** NORMAL HANDLING ON RECORD LEVEL
                    '*************************************************************

                    ' get the compounds
                    aTable = GetTableStore(aTableName)
                    aCompRecordColl = aTable.GetRecordsByIndex(ConstDefaultCompoundIndexName, keyArray:=pkarry, silent:=True)
                    If aCompRecordColl Is Nothing Then
                        Call CoreMessageHandler(subname:="clsOTDBXChangeConfig.fillMappingWithCompounds", arg1:=ConstDefaultCompoundIndexName,
                                              message:=" the compound index is not found ",
                                               messagetype:=otCoreMessageType.InternalError, tablename:=aTableName)
                        Return False
                    End If

                    '**
                    For Each aRecord In aCompRecordColl
                        aCompField = aRecord.GetValue(compIDFieldname)
                        aCompValue = aRecord.GetValue(compValueFieldName)

                        ' found in Dir
                        If anEntryDir.ContainsKey(key:=UCase(aCompField)) Then

                            'Set aTableEntry = anEntryDir.Item(Key:=LCase(aCompField)) -> should be the same
                            'aConfigmember = Me.AttributeByFieldName(LCase(aCompField))
                            ' map it back -> set values which are not set (e.g. other keys)
                            If Not aConfigmember Is Nothing Then
                                ' save old value
                                If ORIGMAPPING.ContainsKey(key:=aConfigmember.ordinal.Value) Then
                                    Call ORIGMAPPING.Remove(key:=aConfigmember.ordinal.Value)
                                End If
                                Call aConfigmember.convertValue4DB(aCompValue, aVAlue)    '-> MAPPING SHOULD BE HOST DATA

                                Call ORIGMAPPING.Add(key:=aConfigmember.ordinal.Value, value:=aVAlue)

                                ' overload depending otXChangeCommandType.Read and not PrimaryKey or otUpdate
                                ' run the original DB Value (runXCHange makes s 4DB too)
                                Call aConfigmember.runXChange(MAPPING:=MAPPING, VARIABLE:=aCompValue, MSGLOG:=MSGLOG)
                            End If
                        End If
                    Next aRecord
                End If

            Next tablename

            fillMappingWithCompounds = True
        End Function


    End Class

    '************************************************************************************
    '***** CLASS clsOTDBXChangeConfig is the object for a OTDBRecord (which is the datastore)
    '*****       defines a OTDB eXchange Configuration
    '*****

    Public Class clsOTDBXChangeConfig
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        'Implements iOTDBXChange

        <ormSchemaTableAttribute(Version:=2)> Public Const constTableID = "tblXChangeConfigs"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=50, primaryKeyordinal:=1,
             Title:="Name", Description:="Name of XChange Configuration")>
        Public Const constFNID = "configname"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=255,
             Title:="Description", Description:="Description of XChange Configuration")>
        Public Const constFNDesc = "desc"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Memo,
             Title:="Comments", Description:="Comments")>
        Public Const constFNTitle = "cmt"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool,
             Title:="IsDynamic", Description:="the XChange Config accepts dynamic addition of XChangeIDs")>
        Public Const constFNDynamic = "isdynamic"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=50,
               Title:="Outline ID", Description:="ID to the associated Outline")>
        Public Const constFNOutline = "outline"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=255,
              Title:="Message Log Tag", Description:="Message Log Tag")>
        Public Const constFNMsgLogTag = "msglogtag"


        ' fields
        <ormColumnMappingAttribute(fieldname:=constFNID)> Private _configname As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNDesc)> Private _description As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNMsgLogTag)> Private _msglogtag As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNDynamic)> Private _DynamicAttributes As Boolean
        <ormColumnMappingAttribute(fieldname:=constFNOutline)> Private _outlineid As String = ""


        Private _msglog As New ObjectLog
        Private _processedDate As Date = ConstNullDate

        ' members itself per key:=indexnumber, item:=clsOTDBXChangeMember
        Private _members As New SortedDictionary(Of Long, clsOTDBXChangeMember)
        Private _membersByordinal As New SortedDictionary(Of Ordinal, List(Of clsOTDBXChangeMember))

        ' reference object order list to work through members in the row of the exchange
        Private _objectsDirectory As New Dictionary(Of String, clsOTDBXChangeMember)
        Private _objectsByOrderDirectory As New SortedDictionary(Of Long, clsOTDBXChangeMember)

        ' reference Attributes list to work
        Private _attributesIDDirectory As New Dictionary(Of String, clsOTDBXChangeMember)
        Private _attributesByObjectnameDirectory As New Dictionary(Of String, List(Of clsOTDBXChangeMember))
        Private _attributesIDList As New Dictionary(Of String, List(Of clsOTDBXChangeMember)) ' list if IDs are not unique
        Private _aliasDirectory As New Dictionary(Of String, List(Of clsOTDBXChangeMember))

        ' object ordinalMember -> Members which are driving the ordinal of the complete eXchange
        ' Private _orderByMembers As New Dictionary(Of Object, clsOTDBXChangeMember)

        '** dynamic outline
        Dim _outline As New XOutline

        '** initialize
        Public Sub New()
            Call MyBase.New(constTableID)
            'me.record.tablename = ourTableName
            _msglog = New ObjectLog

        End Sub

#Region "Properties"


        ''' <summary>
        ''' Gets or sets the S outlineid.
        ''' </summary>
        ''' <value>The S outlineid.</value>
        Public Property OutlineID() As String
            Get
                Return Me._outlineid
            End Get
            Set(value As String)
                Me._outlineid = value
                _outline = Nothing
            End Set
        End Property
        ReadOnly Property Outline As XOutline
            Get
                If Me._outlineid <> "" And (_IsLoaded Or Me.IsCreated) Then
                    If Not _outline.IsLoaded And Not _outline.IsCreated Then
                        If _outline.LoadBy(Me._outlineid) Then
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
                Me._DynamicAttributes = value
            End Set
        End Property

        '****** getUniqueTag
        Public Function GetUniqueTag()
            getUniqueTag = ConstDelimiter & constTableID & ConstDelimiter & _configname & ConstDelimiter & "0" & ConstDelimiter
        End Function
        ReadOnly Property msglogtag() As String
            Get
                If _msglogtag = "" Then
                    _msglogtag = getUniqueTag()
                End If
                msglogtag = _msglogtag
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

        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                _description = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property ProcessedDate() As Date
            Get
                ProcessedDate = _processedDate
            End Get
            Set(value As Date)
                _processedDate = value
                Me.IsChanged = True
            End Set
        End Property

        ReadOnly Property NoAttributes() As Long
            Get
                NoAttributes = _attributesIDDirectory.Count
            End Get

        End Property

        ReadOnly Property NoObjects() As Long
            Get
                NoObjects = _objectsDirectory.Count
            End Get
        End Property

        ReadOnly Property NoMembers() As Long
            Get
                NoMembers = _members.Count - 1
            End Get
        End Property
#End Region


        ''' <summary>
        '''  get the maximal ordinal as long if it is numeric
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMaxordinalNo() As Long
            If Not IsCreated And Not IsLoaded Then
                GetMaxordinalNo = 0
                Exit Function
            End If

            Return _members.Keys.Max()
        End Function

        ''' <summary>
        ''' returns the maximal index number
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMaxIndexNo() As Long

            If NoMembers >= 0 Then
                Return Me.MemberIndexNo.Max
            Else
                Return 0
            End If

        End Function
        ''' <summary>
        ''' returns the max order number 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMaxObjectOrderNo() As Long
            Dim keys As List(Of Long)

            If NoMembers >= 0 Then
                keys = Me.ObjectOrderNo
                If keys.Count > 0 Then
                    Return keys.Max
                Else
                    Return 0
                End If
            Else
                Return 0
            End If

        End Function


        '*** get the highest need XCMD to run the attributes XCMD
        '***
        Public Function GetHighestXCmd() As otXChangeCommandType

            Dim aHighestXcmd As otXChangeCommandType

            aHighestXcmd = 0

            Dim listofObjects As List(Of clsOTDBXChangeMember) = Me.Objects
            If listofObjects.Count = 0 Then
                Return 0
            End If

            For Each aChangeMember As clsOTDBXChangeMember In listofObjects
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

            Dim listofAttributes As List(Of clsOTDBXChangeMember) = Me.Attributes(objectname:=objectname)
            If listofAttributes.Count = 0 Then
                Return 0
            End If

            For Each aChangeMember As clsOTDBXChangeMember In listofAttributes
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

            GetHighestObjectXCmd = aHighestXcmd
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
        Public Function SetordinalForID(ByVal ID As String, ByVal ordinal As Object, Optional ByVal objectname As String = "") As Boolean
            Dim anEntry As New clsOTDBXChangeMember()
            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                SetordinalForID = False
                Exit Function
            End If

            ' get the entry
            anEntry = Me.AttributeByID(ID, OBJECTNAME)
            If anEntry Is Nothing Then
                Return False
            ElseIf Not anEntry.IsLoaded And Not anEntry.IsCreated Then
                Return False
            End If

            If Not TypeOf ordinal Is OnTrack.Ordinal Then
                ordinal = New Ordinal(ordinal)
            End If
            anEntry.ordinal = ordinal
            AddordinalReference(anEntry)
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
            Dim aMember As New clsOTDBXChangeMember

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                SetObjectXCmd = False
                Exit Function
            End If

            ' return if exists
            If Not _objectsDirectory.ContainsKey(key:=name) Then
                SetObjectXCmd = False
                Exit Function
            Else
                aMember = _objectsDirectory.Item(key:=name)
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

            Dim aMember As New clsOTDBXChangeMember
            Dim anObjectDef As ObjectDefinition = CurrentSession.Objects.GetObject(name)


            ' Nothing
            If Not Me.IsLoaded And Not Me.IsCreated Then
                Return False
            End If

            ' return if exists
            If _objectsDirectory.ContainsKey(key:=LCase(name)) Then
                If xcmd = 0 Then
                    aMember = _objectsDirectory.Item(key:=LCase(name))
                    xcmd = aMember.XChangeCmd
                End If
                Call SetObjectXCmd(name:=name, xchangecommand:=xcmd)
                Return False
            End If

            ' load
            If anObjectDef Is Nothing Then
                Return False
            End If

            ' add the component
            aMember = New clsOTDBXChangeMember
            If aMember.Create(Me.Configname, Me.GetMaxIndexNo + 1) Then
                aMember.ID = ""
                aMember.ordinal.Value = New Ordinal(0)
                aMember.Entryname = ""
                aMember.Objectname = name
                aMember.IsAttributeEntry = False
                aMember.IsObjectEntry = True

                If orderno = 0 Then
                    aMember.Orderno = Me.GetMaxObjectOrderNo + 1
                Else
                    aMember.Orderno = orderno
                End If
                aMember.ordinal.Value = orderno
                If IsMissing(xcmd) Then
                    xcmd = otXChangeCommandType.Read
                End If
                aMember.XChangeCmd = xcmd

                Return Me.AddMember(aMember)
            End If

            Return False

        End Function

        ''' <summary>
        ''' Adds an atribute by fieldname
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <param name="ordinal"></param>
        ''' <param name="OBJECTNAME"></param>
        ''' <param name="ISXCHANGED"></param>
        ''' <param name="XCMD"></param>
        ''' <param name="READ_ONLY"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddAttributeByField(ByRef entryname As String,
                                             ByVal objectname As String,
                                                Optional ByVal ordinal As Object = Nothing,
                                                Optional ByVal isXChanged As Boolean = True,
                                                Optional ByVal xcmd As otXChangeCommandType = Nothing,
                                                Optional ByVal [readonly] As Boolean = False) As Boolean

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then

                AddAttributeByField = False
                Exit Function
            End If

            Dim anFieldEntry As ObjectEntryDefinition = CurrentSession.Objects.GetEntry(objectname:=objectname, entryname:=entryname)


            If Not anFieldEntry Is Nothing Then
                Return Me.AddAttributeByField(objectentry:=anFieldEntry, objectname:=objectname, ordinal:=ordinal, isxchanged:=isXChanged, xcmd:=xcmd, [readonly]:=[readonly])
            Else
                Call CoreMessageHandler(message:="field entry not found", arg1:=objectname & "." & entryname, messagetype:=otCoreMessageType.InternalError,
                                         subname:="clsOTDBXChangeConfig.addAttributeByField")

                Return False
            End If

        End Function
        '*** add a Attribute by an ID
        '***
        Public Function AddAttributeByField(ByRef objectentry As ObjectEntryDefinition,
                                        Optional ByVal ordinal As Object = Nothing,
                                        Optional ByVal objectname As String = "",
                                        Optional ByVal isxchanged As Boolean = True,
                                        Optional ByVal xcmd As otXChangeCommandType = Nothing,
                                        Optional ByVal [readonly] As Boolean = False) As Boolean
            Dim aMember As New clsOTDBXChangeMember
            'Dim FIELDENTRY As New clsOTDBSchemaDefTableEntry
            Dim aVAlue As Object
            Dim objectMember As clsOTDBXChangeMember

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                AddAttributeByField = False
                Exit Function
            End If

            ' load
            If Not objectentry.IsLoaded And Not objectentry.IsCreated Then
                AddAttributeByField = False
                Exit Function
            End If

            ' if ordinal is missing -> create one
            If ordinal Is Nothing Then
                For Each [alias] In objectentry.Aliases
                    'could be more than one Attribute by Alias
                    aMember = Me.AttributeByID(ID:=[alias])
                    If aMember IsNot Nothing Then
                        If aMember.IsLoaded Or aMember.IsCreated Then
                            ordinal = aMember.ordinal
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
                If IsMissing(isxchanged) Then
                    isxchanged = False
                End If
            End If

            '*** Add the Object if necessary
            If objectname = "" Then
                objectMember = Me.ObjectByName(objectentry.Objectname)
                If objectMember Is Nothing Then
                    If Me.AddObjectByName(name:=objectentry.Objectname, xcmd:=xcmd) Then
                        objectMember = Me.ObjectByName(objectentry.Objectname)
                    End If
                End If
            Else
                objectMember = Me.ObjectByName(objectname)
                If objectMember Is Nothing Then
                    If Me.AddObjectByName(name:=objectname, xcmd:=xcmd) Then
                        objectMember = Me.ObjectByName(objectname)
                    End If
                End If
            End If

            '** add a default command -> might be also 0 if object was added with entry
            If xcmd = 0 Then
                xcmd = objectMember.XChangeCmd
            End If


            ' add the component
            aMember = New clsOTDBXChangeMember
            If aMember.Create(Me.Configname, Me.GetMaxIndexNo + 1) Then
                aMember.ID = objectentry.ID
                If Not TypeOf ordinal Is OnTrack.Ordinal Then
                    ordinal = New Ordinal(ordinal)
                End If

                aMember.ordinal = ordinal ' create an ordinal 
                aMember.Entryname = objectentry.name
                aMember.IsXChanged = isxchanged
                aMember.IsReadOnly = [readonly]
                aMember.[ObjectEntryDefinition] = objectentry
                aMember.Objectname = objectMember.Objectname
                aMember.IsAttributeEntry = True
                aMember.IsObjectEntry = False
                aMember.XChangeCmd = xcmd
                ' add the Object too
                Return Me.AddMember(aMember)

            End If

            Return False


        End Function
        ''' <summary>
        ''' Adds an Attribute to the XCHange Config by its XChange-ID
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="ordinal"></param>
        ''' <param name="objectname"></param>
        ''' <param name="isXChanged"></param>
        ''' <param name="xcmd"></param>
        ''' <param name="readonly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddAttributeByID(ByVal id As String,
                                            Optional ByVal ordinal As Object = Nothing,
                                            Optional ByVal objectname As String = "",
                                            Optional ByVal isXChanged As Boolean = True,
                                            Optional ByVal xcmd As otXChangeCommandType = Nothing,
                                            Optional ByVal [readonly] As Boolean = False) As Boolean


            AddAttributeByID = False

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                AddAttributeByID = False
                Exit Function
            End If

            '*** no objectname -> get all IDs in objects
            If objectname = "" Then
                For Each entry In CurrentSession.Objects.GetEntryByID(id:=id)
                    '** compare to objects in order
                    If Me.NoObjects > 0 Then
                        For Each anObjectEntry In Me.ObjectsByOrderNo
                            If LCase(entry.Objectname) = LCase(anObjectEntry.Objectname) Then
                                AddAttributeByID = AddAttributeByField(objectentry:=entry, ordinal:=ordinal,
                                                                  isxchanged:=isXChanged,
                                                                  objectname:=entry.Objectname,
                                                                  xcmd:=xcmd, readonly:=[readonly])
                            End If
                        Next
                        ' simply add

                    Else
                        AddAttributeByID = AddAttributeByField(objectentry:=entry, ordinal:=ordinal,
                                                          isxchanged:=isXChanged,
                                                          objectname:=entry.Objectname, xcmd:=xcmd, readonly:=[readonly])
                    End If

                Next

            Else
                For Each entry In CurrentSession.Objects.GetEntryByID(id:=id)
                    If LCase(objectname) = LCase(entry.Objectname) Then
                        AddAttributeByID = AddAttributeByField(objectentry:=entry, ordinal:=ordinal,
                                                          isxchanged:=isXChanged,
                                                          objectname:=entry.Objectname,
                                                          xcmd:=xcmd, readonly:=[readonly])
                    End If
                Next


            End If

            ' return
            AddAttributeByID = AddAttributeByID Or False
            Exit Function


        End Function
        ''' <summary>
        ''' returns True if an Objectname with an ID exists
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Exists(Optional ByVal objectname As String = "", Optional ByVal ID As String = "") As Boolean
            Dim flag As Boolean

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                Exists = False
                Exit Function
            End If

            ' missing arguments
            If objectname = "" Then
                Call CoreMessageHandler(subname:="clsOTDBXChangeConfig.exists", message:="objectname was not set")
                Exists = False
                Exit Function
            End If
            ' missing arguments
            If objectname = "" And ID = "" Then
                Call CoreMessageHandler(subname:="clsOTDBXChangeConfig.exists", message:="set either objectname or attributename - not both")
                Exists = False
                Exit Function
            End If

            '+ check
            If objectname <> "" And ID = "" Then
                If _objectsDirectory.ContainsKey(key:=objectname) Then
                    Exists = True
                Else
                    Exists = False
                End If
                Exit Function
            Else
                If _attributesIDDirectory.ContainsKey(key:=ID) Then

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
        Private Function AddIDReference(ByRef member As clsOTDBXChangeMember) As Boolean
            Dim entries As List(Of clsOTDBXChangeMember)

            If _attributesIDList.ContainsKey(key:=UCase(member.ID)) Then
                entries = _attributesIDList.Item(UCase(member.ID))
            Else

                entries = New List(Of clsOTDBXChangeMember)
                _attributesIDList.Add(UCase(member.ID), entries)
            End If
            If entries.Contains(member) Then entries.Remove(member)
            entries.Add(member)

            Return True
        End Function
        ''' <summary>
        ''' Add ordinal to Reference Structures
        ''' </summary>
        ''' <param name="member"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AddordinalReference(ByRef member As clsOTDBXChangeMember) As Boolean
            Dim entries As List(Of clsOTDBXChangeMember)
            '** sorted
            If _membersByordinal.ContainsKey(key:=member.ordinal) Then
                entries = _membersByordinal.Item(member.ordinal)
            Else
                entries = New List(Of clsOTDBXChangeMember)
                _membersByordinal.Add(member.ordinal, entries)
            End If

            If entries.Contains(member) Then entries.Remove(member)
            entries.Add(member)

            Return True
        End Function
        ''' <summary>
        ''' Add an Entry by ID
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AddObjectReference(ByRef member As clsOTDBXChangeMember) As Boolean
            Dim entries As List(Of clsOTDBXChangeMember)

            If _AttributesByObjectnameDirectory.ContainsKey(key:=LCase(member.Objectname)) Then
                entries = _AttributesByObjectnameDirectory.Item(LCase(member.Objectname))
            Else
                entries = New List(Of clsOTDBXChangeMember)
                _AttributesByObjectnameDirectory.Add(LCase(member.Objectname), entries)
            End If

            entries.Add(member)

            Return True
        End Function
        ''' <summary>
        ''' Add an Entry by ID
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AddAliasReference(ByRef member As clsOTDBXChangeMember) As Boolean
            Dim entries As List(Of clsOTDBXChangeMember)

            For Each [alias] As String In member.aliases

                If _AliasDirectory.ContainsKey(key:=UCase([alias])) Then
                    entries = _AliasDirectory.Item(key:=UCase([alias]))
                Else
                    entries = New List(Of clsOTDBXChangeMember)
                    _AliasDirectory.Add(key:=UCase([alias]), value:=entries)
                End If
                If entries.Contains(member) Then entries.Remove(member)
                entries.Add(member)
            Next

            Return True
        End Function
        ''' <summary>
        ''' Add XChangeMember
        ''' </summary>
        ''' <param name="anEntry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddMember(anEntry As clsOTDBXChangeMember) As Boolean
            Dim anObjectEntry As New clsOTDBXChangeMember


            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                AddMember = False
                Exit Function
            End If

            ' remove and overwrite
            If _members.ContainsKey(key:=anEntry.indexno) Then
                Call _members.Remove(key:=anEntry.indexno)
            End If

            ' add Member Entry
            _members.Add(key:=anEntry.indexno, value:=anEntry)



            ' Add to the Attribute Section
            If anEntry.isAttributeEntry Then
                ' check on the Object of the Attribute
                If _objectsDirectory.ContainsKey(key:=anEntry.Objectname) Then
                    anObjectEntry = _objectsDirectory.Item(key:=anEntry.Objectname)
                Else
                    anObjectEntry = New clsOTDBXChangeMember
                    Call anObjectEntry.create(Me.Configname, Me.GetMaxIndexNo + 1)
                    anObjectEntry.Objectname = anEntry.Objectname
                    anObjectEntry.isAttributeEntry = False
                    anObjectEntry.isObjectEntry = True
                    anObjectEntry.orderno = Me.GetMaxObjectOrderNo + 1
                    anObjectEntry.xChangeCmd = otXChangeCommandType.Read
                    ' add the object entry
                    If Not AddMember(anObjectEntry) Then
                    End If
                End If

                ' add the Attribute
                If _attributesIDDirectory.ContainsKey(key:=anEntry.ID) Then
                    Call _attributesIDDirectory.Remove(key:=anEntry.ID)
                End If

                Call _attributesIDDirectory.Add(key:=anEntry.ID, value:=anEntry)
                '** references
                AddIDReference(anEntry) '-> List references if multipe
                AddObjectReference(anEntry)
                AddAliasReference(anEntry)
                AddordinalReference(anEntry)
                ' Add to the Object Section
            ElseIf anEntry.isObjectEntry Then
                '**
                If _objectsDirectory.ContainsKey(key:=anEntry.Objectname) Then
                    Call _objectsDirectory.Remove(key:=anEntry.Objectname)
                End If
                Call _objectsDirectory.Add(key:=anEntry.Objectname, value:=anEntry)

                '**
                If _objectsByOrderDirectory.ContainsKey(key:=anEntry.orderno) Then
                    Call _objectsByOrderDirectory.Remove(key:=anEntry.orderno)
                End If
                Call _objectsByOrderDirectory.Add(key:=anEntry.orderno, value:=anEntry)
            End If

            '
            AddMember = True

        End Function
        ''' <summary>
        ''' Initialize
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean
            Initialize = MyBase.Initialize()
            _members = New SortedDictionary(Of Long, clsOTDBXChangeMember)
            _attributesIDDirectory = New Dictionary(Of String, clsOTDBXChangeMember)
            _objectsDirectory = New Dictionary(Of String, clsOTDBXChangeMember)
            _DynamicAttributes = False
            _description = ""
            _configname = ""
            _processedDate = ConstNullDate

        End Function
        ''' <summary>
        ''' Resets all dynamic structures
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function Reset() As Boolean
            _objectsDirectory.Clear()
            _objectsByOrderDirectory.Clear()
            _attributesIDDirectory.Clear()
            _attributesByObjectnameDirectory.Clear()
            _attributesIDList.Clear()
            _aliasDirectory.Clear()
            _members.Clear()
            _membersByordinal.Clear()
        End Function
        ''' <summary>
        ''' deletes an objects in persistency store
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Delete() As Boolean
            Dim anEntry As clsOTDBXChangeMember


            If Not Me.IsCreated And Not _IsLoaded Then
                Delete = False
                Exit Function
            End If

            ' delete each entry
            For Each anEntry In _members.Values
                anEntry.Delete()
            Next
            MyBase.Delete()

            ' reset it
            Me.Reset()

            _IsCreated = True
            Me.IsDeleted = True
            Me.Unload()

        End Function

        ''' <summary>
        ''' retrieves an Object by its name or nothing
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ObjectByName(ByVal objectname As String) As clsOTDBXChangeMember

            If _objectsDirectory.ContainsKey(LCase(OBJECTNAME)) Then
                Return _objectsDirectory.Item(key:=LCase(OBJECTNAME))
            Else
                Return Nothing
            End If

        End Function
        '**** ObjectOrderno returns an Object array of orderno's
        '****
        Public Function ObjectOrderNo() As IEnumerable
            Return _objectsByOrderDirectory.Keys.ToList
        End Function
        ''' <summary>
        ''' retrieves a list of the Index Numbers of the members
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function MemberIndexNo() As List(Of Long)

            If Not Me.IsCreated And Not _IsLoaded Then
                Return New List(Of Long)
            End If

            Return _members.Keys.ToList

        End Function
        ''' <summary>
        ''' retrieves the ordinal numbers of the objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ObjectsByOrderNo() As IEnumerable(Of clsOTDBXChangeMember)


            If Not Me.IsCreated And Not _IsLoaded Then
                Return New List(Of clsOTDBXChangeMember)
            End If

            Return _objectsByOrderDirectory.Values
        End Function

        ''' <summary>
        ''' retrieves a List of Attributes per Objectname
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AttributesByObjectName(ByVal objectname As String) As IEnumerable(Of clsOTDBXChangeMember)

            If _AttributesByObjectnameDirectory.ContainsKey(objectname) Then
                Return _AttributesByObjectnameDirectory.Item(key:=objectname)
            Else
                Return New List(Of clsOTDBXChangeMember)
            End If


        End Function

        '**** Members returns a Collection of Members in Order of the IndexNo
        '****
        Public Function MembersByIndexNo() As IEnumerable

            Return Me._members.Values
        End Function

        '**** Members returns a Collection of Members
        '****
        Public Function Members() As List(Of clsOTDBXChangeMember)
            Dim aCollection As New List(Of clsOTDBXChangeMember)

            If Not Me.IsCreated And Not _IsLoaded Then
                Return aCollection
                Exit Function
            End If


            For Each anEntry As clsOTDBXChangeMember In _members.Values
                If (anEntry.ID <> "") And (anEntry.Objectname <> "") Then
                    aCollection.Add(anEntry)
                End If
            Next


            Return aCollection
        End Function
        ''' <summary>
        ''' returns all the objectMembers
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Objects() As List(Of clsOTDBXChangeMember)
            Dim aCollection As New List(Of clsOTDBXChangeMember)

            If Not Me.IsCreated And Not _IsLoaded Then
                Return aCollection
            End If

            For Each anEntry As clsOTDBXChangeMember In _objectsDirectory.Values
                If (anEntry.Objectname <> "") Then
                    aCollection.Add(anEntry)
                End If
            Next

            Return aCollection
        End Function
        ''' <summary>
        ''' returns an attribute by its fieldname and tablename
        ''' </summary>
        ''' <param name="fieldname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AttributeByFieldName(ByVal fieldname As String,
                                            Optional ByVal tablename As String = "") As clsOTDBXChangeMember

            Dim aMember As clsOTDBXChangeMember
            If Not Me.IsCreated And Not _IsLoaded Then
                AttributeByFieldName = Nothing
                Exit Function
            End If
            Dim alist As List(Of clsOTDBXChangeMember)
            If tablename <> "" Then

                '* might be we have the object but no fields
                If _attributesByObjectnameDirectory.ContainsKey(key:=LCase(tablename)) Then
                    alist = _attributesByObjectnameDirectory.Item(key:=LCase(tablename))
                    aMember = alist.Find(Function(m As clsOTDBXChangeMember)
                                             Return LCase(m.Entryname) = LCase(fieldname)
                                         End Function)

                    If Not aMember Is Nothing Then
                        Return aMember
                    End If
                End If

            Else
                For Each objectdef In _objectsByOrderDirectory.Values
                    If _attributesByObjectnameDirectory.ContainsKey(key:=objectdef.Objectname) Then
                        alist = _attributesByObjectnameDirectory(key:=objectdef.Objectname)

                        aMember = alist.Find(Function(m As clsOTDBXChangeMember)
                                                 Return LCase(m.Entryname) = LCase(fieldname)
                                             End Function)

                        If Not aMember Is Nothing Then
                            Return aMember
                        End If
                    End If
                Next
            End If

            '** search also by ID and consequent by ALIAS
            Dim anObjectEntry As ObjectEntryDefinition = CurrentSession.Objects.GetEntry(objectname:=tablename, entryname:=fieldname)
            If Not anObjectEntry Is Nothing Then
                aMember = Me.AttributeByID(ID:=anObjectEntry.ID, objectname:=tablename)
                If Not aMember Is Nothing Then
                    Return aMember
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
        Public Function AttributeByID(ByVal ID As String,
                                        Optional ByVal objectname As String = "") As clsOTDBXChangeMember

            Dim aCollection As IEnumerable


            If Not Me.IsCreated And Not _IsLoaded Then
                AttributeByID = Nothing
                Exit Function
            End If

            If _attributesIDList.ContainsKey(UCase(ID)) Then
                aCollection = _attributesIDList.Item(UCase(ID))
                For Each entry As clsOTDBXChangeMember In aCollection
                    If objectname <> "" AndAlso LCase(entry.Objectname) = LCase(objectname) Then
                        Return entry
                    ElseIf objectname = "" Then
                        Return entry
                    End If
                Next

            End If

            '** look into aliases 
            '**
            '* check if ID is an ID already in the xconfig
            AttributeByID = AttributeByAlias(ID, objectname)
            If AttributeByID Is Nothing Then
                '* check all Objects coming through with this ID
                For Each anObjectEntry In CurrentSession.Objects.GetEntryByID(id:=ID)
                    '** check on all the XConfig Objects
                    For Each anObjectMember In Me.ObjectsByOrderNo
                        '* if ID is included as Alias Name
                        AttributeByID = AttributeByAlias(alias:=anObjectEntry.ID, objectname:=anObjectMember.Objectname)
                        '** or the aliases are included in this XConfig
                        If AttributeByID Is Nothing Then
                            For Each aliasID In anObjectEntry.Aliases
                                AttributeByID = AttributeByAlias(alias:=aliasID, objectname:=anObjectMember.Objectname)
                                '* found
                                If Not AttributeByID Is Nothing Then
                                    Exit For
                                End If
                            Next

                        End If
                        '* found
                        If Not AttributeByID Is Nothing Then
                            Exit For
                        End If
                    Next
                    '* found
                    If Not AttributeByID Is Nothing Then
                        Exit For
                    End If
                Next

            End If
            Return AttributeByID
        End Function

        ''' <summary>
        ''' returns an Attribute by its XChange Alias ID
        ''' </summary>
        ''' <param name="alias"></param>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AttributeByAlias(ByVal [alias] As String,
                                        Optional ByVal objectname As String = "") As clsOTDBXChangeMember

            Dim aCollection As IEnumerable


            If Not Me.IsCreated And Not _IsLoaded Then
                AttributeByAlias = Nothing
                Exit Function
            End If

            If _AliasDirectory.ContainsKey(UCase([alias])) Then

                aCollection = _AliasDirectory.Item(UCase([alias]))
                For Each entry As clsOTDBXChangeMember In aCollection
                    If objectname <> "" AndAlso LCase(entry.Objectname) = LCase(objectname) Then
                        Return entry
                    ElseIf objectname = "" Then
                        Return entry
                    End If
                Next

            End If

            Return Nothing
        End Function
        '**** AttributeByAliasID returns a Collection of Attributes
        '****
        Public Function AttributeByRelationID(ByVal ID As String,
        Optional ByVal tablename As String = "") As clsOTDBXChangeMember
            Dim anEntry As New clsOTDBXChangeMember
            Dim aCollection As New Collection
            Dim m As Object
            Dim i, j As Integer
            Dim relationID As Object

            If Not Me.IsCreated And Not _IsLoaded Then
                AttributeByRelationID = Nothing
                Exit Function
            End If

            If Not IsMissing(tablename) Then
                tablename = CStr(tablename)
            Else
                tablename = ""
            End If

            ' return each
            For Each kvp As KeyValuePair(Of String, clsOTDBXChangeMember) In _attributesIDDirectory
                anEntry = kvp.Value

                relationID = anEntry.relation
                '** check all relations
                If IsArrayInitialized(relationID) Then
                    If anEntry.ID <> "" And LCase(relationID(j)) = LCase(ID) Then
                        If (tablename <> "" And LCase(anEntry.Objectname) = LCase(tablename)) Or tablename = "" Then
                            AttributeByRelationID = anEntry
                            Exit Function
                        End If
                    End If
                Else
                    If anEntry.ID <> "" And LCase(relationID) = LCase(ID) Then
                        If (tablename <> "" And LCase(anEntry.Objectname) = LCase(tablename)) Or tablename = "" Then
                            AttributeByRelationID = anEntry
                            Exit Function
                        End If
                    End If
                End If
            Next


            AttributeByRelationID = Nothing
        End Function
        ''' <summary>
        ''' Returns an ienumerable of all attributes (optional just by an objectname)
        ''' </summary>
        ''' <param name="objectname">optional objectname</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Attributes(Optional objectname As String = "") As IEnumerable(Of clsOTDBXChangeMember)
            If Not Me.IsCreated And Not _IsLoaded Then
                Return New List(Of clsOTDBXChangeMember)
            End If

            If objectname <> "" Then
                Return AttributesByObjectName(objectname)
            Else
                Return _attributesIDDirectory.Values.ToList
            End If

        End Function
        ''' <summary>
        ''' Loads a XChange Configuration from Store
        ''' </summary>
        ''' <param name="configname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal configname As String) As Boolean
            Dim aTable As iormDataStore
            Dim aRecordCollection As List(Of ormRecord)
            Dim aRecord As ormRecord
            Dim anEntry As New clsOTDBXChangeMember

            Dim pkarry(0) As Object

            '* lazy init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    LoadBy = False
                    Exit Function
                End If
            End If

            Try
                ' set the primaryKey
                pkarry(0) = LCase(configname)

                '** load
                If MyBase.LoadBy(pkarry) Then
                    ' load the members
                    aTable = GetTableStore(anEntry.TableID)
                    Dim aCommand As ormSqlSelectCommand = aTable.CreateSqlSelectCommand(id:="loadby")
                    If Not aCommand.Prepared Then
                        aCommand.Where = clsOTDBXChangeMember.constFNID & " = @" & clsOTDBXChangeMember.constFNID
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@" & constFNID, fieldname:=clsOTDBXChangeMember.constFNID, tablename:=clsOTDBXChangeMember.constTableID))
                        aCommand.OrderBy = clsOTDBXChangeMember.constFNIDNo & " asc"
                        aCommand.Prepare()
                    End If
                    If aCommand.Prepared Then
                        aCommand.SetParameterValue(ID:="@" & clsOTDBXChangeMember.constFNID, value:=configname)
                    End If
                    aRecordCollection = aCommand.RunSelect

                    ' record collection
                    _configname = configname

                    ' records read
                    For Each aRecord In aRecordCollection
                        ' add the Entry as Component (even the header -> )
                        anEntry = New clsOTDBXChangeMember
                        If anEntry.Infuse(aRecord) Then
                            If Not Me.AddMember(anEntry) Then
                                CoreMessageHandler(message:="couldnot add member", subname:="clsOTDBXChangeConfig.loadby",
                                                   messagetype:=otCoreMessageType.InternalError)
                            End If
                        End If
                    Next
                    '
                    _IsLoaded = True

                End If

                Return True


            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="clsOTDBXchangConfig.Loadby")
                Me.Unload()
                Return False
            End Try


        End Function

        ''' <summary>
        ''' perstist the XChange Config to the datastore
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As DateTime = ConstNullDate) As Boolean
            Persist = MyBase.Persist
            If Persist Then
                ' persist each entry
                If _members.Count > 0 Then
                    For Each anEntry As clsOTDBXChangeMember In _members.Values
                        If anEntry.IsAttributeEntry Or anEntry.IsObjectEntry Then Persist = Persist And anEntry.Persist(timestamp)
                    Next
                End If
                Return Persist
            End If

            Return False

        End Function

        ''' <summary>
        ''' create a persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateSchema(Of clsOTDBXChangeConfig)()

            Dim aFieldDesc As New ormFieldDescription
            Dim PrimaryColumnNames As New Collection
            Dim aTable As New ObjectDefinition


            aFieldDesc.ID = ""
            aFieldDesc.Parameter = ""
            aFieldDesc.Tablename = constTableID

            With aTable
                .Create(constTableID)
                .Delete()

                '***
                '*** Fields
                '****

                'Type
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = constFNID
                aFieldDesc.ColumnName = constFNID
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)


                'description
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "description"
                aFieldDesc.ColumnName = "desc"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                'comment
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "comment"
                aFieldDesc.ColumnName = "cmt"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                'dynamic
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is dynamic attribute"
                aFieldDesc.ColumnName = "isdynamic"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                '
                'outline
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "outline id"
                aFieldDesc.ColumnName = "outline"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' msglogtag
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "message log tag"
                aFieldDesc.ColumnName = "msglogtag"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.ColumnName = ConstFNCreatedOn
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' Index
                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
                ' persist
                .Persist()
                ' change the database
                .AlterSchema()
            End With

            CreateSchema = True
            Exit Function

            ' Handle the error
error_handle:
            Call CoreMessageHandler(subname:="clsOTDBXChangeConfig.createSchema")
            CreateSchema = False
        End Function
        ''' <summary>
        ''' creates a persistable object with primary key
        ''' </summary>
        ''' <param name="configname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal configname As String) As Boolean
            Dim primarykey() As Object = {LCase(configname)}

            If MyBase.Create(primarykey, checkUnique:=True) Then
                ' set the primaryKey
                _configname = LCase(configname)
            End If

            Return Me.IsCreated
        End Function

        '***** runXChange aMapping is aDictionay with key as ordinal and value
        '*****
        Public Function RunXChange(ByRef aMapping As Dictionary(Of Object, Object),
                                    Optional ByRef msglog As ObjectLog = Nothing,
                                    Optional ByVal suspendoverload As Boolean = True) As Boolean 'Implements iOTDBXChange.runXChange

            Dim flag As Boolean
            Dim aTarget As New Target
            Dim aSchedule As New Schedule
            Dim aDeliverable As New Deliverable

            If Not IsLoaded And Not IsCreated Then
                RunXChange = False
                Exit Function
            End If

            ' set msglog
            If msglog Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                msglog = _msglog
                msglog.Create(Me.msglogtag)
            End If

            ' suspend Overloading
            If suspendoverload Then Call SuspendOverloading(True)

            If Me.ProcessedDate = ConstNullDate Then
                Me.ProcessedDate = Now
            End If

            '* go through each object
            For Each anObject In Me.ObjectsByOrderNo

                ' special handling for special objects
                Select Case LCase(anObject.Objectname)

                    ' currschedules
                    Case LCase(CurrentSchedule.ConstTableID)
                        flag = True

                        ' Tracks
                    Case LCase(Track.constTableID)
                        flag = True

                        ' HACK: CARTYPES
                    Case "tblconfigs"
                        flag = flag And aDeliverable.runCartypesXChange(aMapping, Me, msglog)

                        ' schedules
                    Case LCase(Schedule.constTableID)
                        flag = flag And aSchedule.runXChangeOLD(aMapping, Me, msglog)

                        ' Targets
                    Case LCase(Target.constTableID)
                        flag = flag And aTarget.runXChangeOLD(aMapping, Me, msglog)

                    Case Else
                        ' default
                        flag = flag And runDefaultXChange4Object(anObject, aMapping, msglog)
                End Select
            Next

            ' suspend Overloading
            If suspendoverload Then Call SuspendOverloading(False)

            Return True
        End Function

        '***** runXChange4Object
        '*****
        Public Function runDefaultXChange4Object(ByRef XCHANGEOBJECT As clsOTDBXChangeMember,
        ByRef MAPPING As Dictionary(Of Object, Object),
        Optional ByRef MSGLOG As ObjectLog = Nothing,
        Optional ByVal NoCompounds As Boolean = False) As Boolean
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim PkFields As New Dictionary(Of String, ObjectEntryDefinition)
            Dim OLDVALUES As New Dictionary(Of Object, Object)
            Dim aConfigmember As clsOTDBXChangeMember
            Dim anObjectEntry As ObjectEntryDefinition
            Dim pkarry() As Object
            Dim m As Object
            Dim Value As Object
            Dim outvalue As Object
            Dim aDefTable As ObjectDefinition
            Dim persistflag As Boolean
            Dim CreateNewFlag As Boolean
            Dim aliases As Object
            Dim i As Integer
            Dim PrimaryConfigMembers As New List(Of clsOTDBXChangeMember)

            persistflag = False
            If Not IsLoaded And Not IsCreated Then
                runDefaultXChange4Object = False
                Exit Function
            End If

            ' set msglog
            If MSGLOG Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                MSGLOG = _msglog
                MSGLOG.Create(Me.msglogtag)
            End If

            '* build the primary key
            '*
            aDefTable = XCHANGEOBJECT.[ObjectDefinition]
            If aDefTable Is Nothing Then
                WriteLine("???")
                Call CoreMessageHandler(message:="couldnot load schema of table in config " & XCHANGEOBJECT.Configname,
                                      arg1:=XCHANGEOBJECT.Objectname,
                                      subname:="clsOTDBXChangeConfig.runDefaultXChange4Object")
                runDefaultXChange4Object = False
            End If

            ' save the primary fields
            For Each m In aDefTable.GetPrimaryKeyFieldNames
                anObjectEntry = aDefTable.GetEntry(m)
                Call PkFields.Add(value:=anObjectEntry, key:=m)
            Next m

            If PkFields.Count = 0 Then
                WriteLine("??? - no primary keys")
                Call CoreMessageHandler(message:="primary key of table is Nothing in xchange config:" & XCHANGEOBJECT.Configname,
                                      arg1:=aDefTable.name,
                                      subname:="clsOTDBXChangeConfig.runDefaultXChange4Object")
                runDefaultXChange4Object = False
            End If

            OLDVALUES = New Dictionary(Of Object, Object)



            '** field the primary key structure
            '**
            ReDim pkarry(0 To PkFields.Count - 1)
            For Each anObjectEntry In PkFields.Values

                aConfigmember = Me.AttributeByFieldName(fieldname:=anObjectEntry.name,
                                                        tablename:=XCHANGEOBJECT.Objectname)
                ' if no Configmember or no value of ConfigMember -> look for aliases
                If aConfigmember Is Nothing OrElse Not MAPPING.ContainsKey(key:=aConfigmember.ordinal.Value) Then
                    ' check for aliases
                    aliases = anObjectEntry.Aliases
                    For i = LBound(aliases) To UBound(aliases)
                        aConfigmember = Me.AttributeByID(ID:=aliases(i))
                        If Not aConfigmember Is Nothing Then
                            Exit For
                        End If
                    Next i
                End If
                ' still nothing ?!
                If aConfigmember Is Nothing Then
                    Call CoreMessageHandler(message:="a primary key is not a xchange config member of " & XCHANGEOBJECT.Configname,
                                          arg1:=anObjectEntry.name, entryname:=anObjectEntry.name, tablename:=XCHANGEOBJECT.Objectname,
                                          subname:="clsOTDBXChangeConfig.runDefaultXChange4Object", break:=False)
                    Return False
                ElseIf Not MAPPING.ContainsKey(key:=aConfigmember.ordinal.Value) Then
                    Call CoreMessageHandler(message:="a primary key is a xchange config member of " & XCHANGEOBJECT.Configname & " but has no value in the mapping",
                                          arg1:=anObjectEntry.name, entryname:=anObjectEntry.name, tablename:=XCHANGEOBJECT.Objectname,
                                          subname:="clsOTDBXChangeConfig.runDefaultXChange4Object", break:=False)
                    Return False
                Else
                    ' add to list
                    PrimaryConfigMembers.Add(aConfigmember)
                    '** member
                    If MAPPING.ContainsKey(key:=aConfigmember.ordinal.Value) Then
                        Value = MAPPING.Item(key:=aConfigmember.ordinal.Value)
                        ' save old value
                        outvalue = Nothing
                        If aConfigmember.convertValue2DB(inValue:=Value, outvalue:=outvalue, MSGLOG:=MSGLOG, existingValue:=False) Then
                            If Not OLDVALUES.ContainsKey(key:=aConfigmember.ordinal.Value) Then
                                Call OLDVALUES.Add(key:=aConfigmember.ordinal.Value, value:=outvalue)
                            End If
                            pkarry(anObjectEntry.IndexPosition - 1) = outvalue
                        Else
                            Call CoreMessageHandler(message:="a primary key is a xchange config member of " & XCHANGEOBJECT.Configname & " but value couldn't converted",
                                                  arg1:=anObjectEntry.name, entryname:=anObjectEntry.name, tablename:=XCHANGEOBJECT.Objectname,
                                                  subname:="clsOTDBXChangeConfig.runDefaultXChange4Object", break:=False)
                            Return False
                        End If
                    Else
                        Call CoreMessageHandler(message:="a primary key is a xchange config member of " & XCHANGEOBJECT.Configname _
                                              & " but has no value in the mapping",
                                              arg1:=anObjectEntry.name, entryname:=anObjectEntry.name, tablename:=XCHANGEOBJECT.Objectname,
                                              subname:="clsOTDBXChangeConfig.runDefaultXChange4Object", break:=False)
                        Return False
                    End If

                End If

            Next

            '*** read the data
            '***
            aTable = GetTableStore(XCHANGEOBJECT.Objectname)

            aRecord = aTable.GetRecordByPrimaryKey(pkarry)
            If Not aRecord Is Nothing Then
                '*** load the record fields not the compounds !
                '***
                For Each m In aRecord.Keys
                    Value = aRecord.GetValue(m)
                    aConfigmember = Me.AttributeByFieldName(m, tablename:=XCHANGEOBJECT.Objectname)
                    ' map it back -> set values which are not set (e.g. other keys)
                    If Not aConfigmember Is Nothing Then
                        If Not aConfigmember.IsCompound Then
                            outvalue = Nothing
                            Call aConfigmember.convertValue4DB(Value, outvalue)
                            ' save old value
                            If Not OLDVALUES.ContainsKey(key:=aConfigmember.ordinal.Value) Then
                                Call OLDVALUES.Add(key:=aConfigmember.ordinal.Value, value:=outvalue)
                            End If
                            ' add the values to the Mapping
                            ' only if not exists -> might be that we have a key in here for other objects
                            ' depending on the order
                            If Not MAPPING.ContainsKey(key:=aConfigmember.ordinal.Value) And Not IsEmpty(outvalue) Then
                                Call MAPPING.Add(key:=aConfigmember.ordinal.Value, value:=outvalue)
                            End If
                        End If
                    End If
                Next m

                '*** load the compounds
                '***
                If Not NoCompounds Then
                    Call fillMappingWithCompounds(RECORD:=aRecord, MAPPING:=MAPPING, ORIGMAPPING:=OLDVALUES,
                                                  TABLE:=aDefTable, MSGLOG:=MSGLOG)
                End If
                CreateNewFlag = False
            Else
                'Debug.Assert False
                CreateNewFlag = True
            End If

            ' *** run the command
            Select Case XCHANGEOBJECT.XChangeCmd


                '*** delete
                '***
                Case otXChangeCommandType.Delete

                    '**** add or update
                    '****
                Case otXChangeCommandType.Update, otXChangeCommandType.UpdateCreate
                    ' return or create new record
                    If XCHANGEOBJECT.XChangeCmd = otXChangeCommandType.Update And aRecord Is Nothing Then
                        runDefaultXChange4Object = False
                        Exit Function
                    ElseIf XCHANGEOBJECT.XChangeCmd = otXChangeCommandType.UpdateCreate And aRecord Is Nothing Then
                        aRecord = New ormRecord
                        aRecord.SetTable(aTable.TableID, fillDefaultValues:=True)
                        '** set to updateCreate
                        For Each aConfigmember In PrimaryConfigMembers
                            aConfigmember.XChangeCmd = otXChangeCommandType.UpdateCreate
                            aConfigmember.IsXChanged = True
                        Next
                    End If
                    '*** check each Entry of Schema
                    '*** -> only Fields no Compounds
                    For Each m In aDefTable.Entries
                        anObjectEntry = m

                        If anObjectEntry.ID <> "" And anObjectEntry.IsField Then
                            aConfigmember = Me.AttributeByFieldName(anObjectEntry.name, tablename:=anObjectEntry.Objectname)
                            If Not aConfigmember Is Nothing Then
                                If aConfigmember.IsXChanged And
                                (aConfigmember.XChangeCmd = otXChangeCommandType.Update Or aConfigmember.XChangeCmd = otXChangeCommandType.UpdateCreate) Then
                                    ' we have a value AND update !
                                    ' take values from oldvalues
                                    outvalue = Nothing
                                    If aConfigmember.runXChange(MAPPING:=MAPPING,
                                                                VARIABLE:=outvalue,
                                                                oldValuesMap:=OLDVALUES, MSGLOG:=MSGLOG, FORCE:=CreateNewFlag) Then
                                        If aConfigmember.XChangeCmd <> otXChangeCommandType.Read And outvalue <> Nothing Then
                                            persistflag = True
                                            Call aRecord.SetValue(anObjectEntry.name, outvalue)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next m
                    ' save it
                    If persistflag Then
                        runDefaultXChange4Object = aRecord.Persist
                        'writeline( "save"
                    End If
                    Exit Function

                    '*** duplicate
                    '***
                Case otXChangeCommandType.Duplicate

                    '***
                    '*** just read and return
                Case otXChangeCommandType.Read
                    ' overload from oldvalues of this object

                    For Each m In OLDVALUES.Keys
                        Value = OLDVALUES.Item(m)
                        '*** donot overwrite ! -> Order of objects necessary for multiple id values
                        If Not MAPPING.ContainsKey(key:=m) Then
                            Call MAPPING.Add(key:=m, value:=Value)
                        End If

                    Next m
                    runDefaultXChange4Object = Not aRecord Is Nothing
                    Exit Function

                    '**** no command ?!
                Case Else
                    WriteLine("no cmd")
                    Assert(False)
            End Select


        End Function

        '***** fillMappingWithCompounds
        '*****
        Private Function fillMappingWithCompounds(ByRef RECORD As ormRecord,
        ByRef MAPPING As Dictionary(Of Object, Object),
        ByRef ORIGMAPPING As Dictionary(Of Object, Object),
        ByRef TABLE As ObjectDefinition,
        Optional ByRef MSGLOG As ObjectLog = Nothing) As Boolean
            Dim aConfigmember As clsOTDBXChangeMember
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim aCompRecordColl As New List(Of ormRecord)
            Dim aCompTableDir As New Dictionary(Of String, Dictionary(Of String, ObjectEntryDefinition))
            Dim compoundKeys As Object
            Dim aCompField As Object
            Dim aCompValue As Object
            Dim tablename As Object
            Dim aTableEntry As ObjectEntryDefinition
            Dim anEntryDir As New Dictionary(Of String, ObjectEntryDefinition)
            Dim aKey As String
            Dim pkarry() As Object
            Dim i As Integer
            Dim m As Object
            Dim aTableName As String
            Dim compValueFieldname As String
            Dim compIDFieldname As String
            Dim aVAlue As Object

            Dim aSchedule As New Schedule
            Dim aScheduleMilestone As New ScheduleMilestone
            Dim specialHandling As Boolean

            ' store each Compound
            For Each m In TABLE.Entries
                aTableEntry = m
                If aTableEntry.ID <> "" And aTableEntry.IsCompound Then
                    If aCompTableDir.ContainsKey(key:=aTableEntry.CompoundTablename) Then
                        anEntryDir = aCompTableDir.Item(key:=aTableEntry.CompoundTablename)
                    Else
                        anEntryDir = New Dictionary(Of String, ObjectEntryDefinition)
                        Call aCompTableDir.Add(key:=aTableEntry.CompoundTablename, value:=anEntryDir)
                    End If
                    ' add the Entry
                    If Not anEntryDir.ContainsKey(key:=aTableEntry.ID) Then
                        Call anEntryDir.Add(key:=UCase(aTableEntry.ID), value:=aTableEntry)
                    Else
                        Assert(False)

                    End If
                End If
            Next m

            '**********************************************************
            '**** SPECIAL HANDLING OF tblschedules -> Milestones
            '**********************************************************
            If LCase(TABLE.name) = LCase(aSchedule.TableID) Then
                Dim anUID As Long
                Dim anUPDC As Long

                If Not IsNull(RECORD.GetValue("uid")) Then
                    anUID = CLng(RECORD.GetValue("uid"))
                Else
                    anUID = 0
                End If
                If Not IsNull(RECORD.GetValue("updc")) Then
                    anUPDC = CLng(RECORD.GetValue("updc"))
                Else
                    anUPDC = 0
                End If
                ' found
                If anUPDC <> 0 And anUID <> 0 Then
                    If aSchedule.loadBy(UID:=anUID, updc:=anUPDC) Then
                        specialHandling = True
                    Else
                        specialHandling = False
                    End If
                Else
                    specialHandling = False
                    'Debug.Print("mmh no schedule for ", anUID, anUPDC)
                End If
            Else
                specialHandling = False
            End If

            '*** for each compound table
            '***
            For Each tablename In aCompTableDir.Keys

                ' get the Entries
                aTableName = CStr(tablename)
                anEntryDir = aCompTableDir.Item(key:=aTableName)
                aTableEntry = anEntryDir.First.Value      'first item
                compIDFieldname = aTableEntry.CompoundIDFieldname
                compValueFieldname = aTableEntry.CompoundValueFieldname

                ' look up the keys
                compoundKeys = aTableEntry.CompoundRelation
                If Not IsArrayInitialized(compoundKeys) Then
                    Call CoreMessageHandler(arg1:=aTableEntry.name, message:="no compound relation found for fieldname", subname:="clsOTDBXchangeConfig.fillMappingWithCompounds")
                    fillMappingWithCompounds = False
                    Exit Function
                End If
                ReDim pkarry(UBound(compoundKeys))
                For i = LBound(compoundKeys) To UBound(compoundKeys)
                    pkarry(i) = RECORD.GetValue(index:=compoundKeys(i))
                Next i


                '**********************************************************
                '**** SPECIAL HANDLING OF tblschedules -> Milestones
                '**********************************************************
                If LCase(aTableName) = LCase(ScheduleMilestone.constTableID) And specialHandling Then

                    For Each aTableEntry In TABLE.Entries
                        'aTableEntry = m
                        If aTableEntry.ID <> "" And aTableEntry.IsCompound Then
                            aCompValue = aSchedule.GetMilestoneValue(ID:=aTableEntry.ID)
                            'Set aTableEntry = anEntryDir.Item(Key:=LCase(aCompField)) -> should be the same
                            aConfigmember = Me.AttributeByFieldName(fieldname:=aTableEntry.ID, tablename:=aTableEntry.Objectname)
                            aVAlue = Nothing
                            ' map it back -> set values which are not set (e.g. other keys)
                            If Not aConfigmember Is Nothing Then
                                ' save old value
                                If ORIGMAPPING.ContainsKey(key:=aConfigmember.ordinal.Value) Then
                                    Call ORIGMAPPING.Remove(key:=aConfigmember.ordinal.Value)
                                End If
                                Call aConfigmember.convertValue4DB(aCompValue, aVAlue)    '-> MAPPING SHOULD BE HOST DATA
                                Call ORIGMAPPING.Add(key:=aConfigmember.ordinal.Value, value:=aVAlue)

                                ' overload depending otRead and not PrimaryKey or otUpdate
                                ' run the original DB Value (runXCHange makes s 4DB too)
                                Call aConfigmember.runXChange(MAPPING:=MAPPING, VARIABLE:=aCompValue, MSGLOG:=MSGLOG)

                            End If
                        End If
                    Next

                Else
                    '*************************************************************
                    '***** NORMAL HANDLING ON RECORD LEVEL
                    '*************************************************************

                    ' get the compounds
                    aTable = GetTableStore(aTableName)
                    aCompRecordColl = aTable.GetRecordsByIndex(ConstDefaultCompoundIndexName, keyArray:=pkarry, silent:=True)
                    If aCompRecordColl Is Nothing Then
                        Call CoreMessageHandler(subname:="clsOTDBXChangeConfig.fillMappingWithCompounds", arg1:=ConstDefaultCompoundIndexName,
                                              message:=" the compound index is not found ",
                                               messagetype:=otCoreMessageType.InternalError, tablename:=aTableName)
                        Return False
                    End If

                    '**
                    For Each aRecord In aCompRecordColl
                        aCompField = aRecord.GetValue(compIDFieldname)
                        aCompValue = aRecord.GetValue(compValueFieldname)

                        ' found in Dir
                        If anEntryDir.ContainsKey(key:=UCase(aCompField)) Then

                            'Set aTableEntry = anEntryDir.Item(Key:=LCase(aCompField)) -> should be the same
                            aConfigmember = Me.AttributeByFieldName(LCase(aCompField))
                            ' map it back -> set values which are not set (e.g. other keys)
                            If Not aConfigmember Is Nothing Then
                                ' save old value
                                If ORIGMAPPING.ContainsKey(key:=aConfigmember.ordinal.Value) Then
                                    Call ORIGMAPPING.Remove(key:=aConfigmember.ordinal.Value)
                                End If
                                Call aConfigmember.convertValue4DB(aCompValue, aVAlue)    '-> MAPPING SHOULD BE HOST DATA

                                Call ORIGMAPPING.Add(key:=aConfigmember.ordinal.Value, value:=aVAlue)

                                ' overload depending otXChangeCommandType.Read and not PrimaryKey or otUpdate
                                ' run the original DB Value (runXCHange makes s 4DB too)
                                Call aConfigmember.runXChange(MAPPING:=MAPPING, VARIABLE:=aCompValue, MSGLOG:=MSGLOG)
                            End If
                        End If
                    Next aRecord
                End If

            Next tablename

            fillMappingWithCompounds = True
        End Function

        '***** runXPreCheck on aMapping is aDictionay with key as ordinal and value
        '*****
        Public Function RunXPreCheck(ByRef aMapping As Dictionary(Of Object, Object),
        Optional ByRef MSGLOG As ObjectLog = Nothing,
        Optional ByVal SUSPENDOVERLOAD As Boolean = True) As Boolean 'Implements iOTDBXChange.runXPreCheck
            Dim anObject As clsOTDBXChangeMember
            Dim m As Object
            Dim flag As Boolean
            Dim aTarget As New Target
            Dim adeliverable As New Deliverable

            If Not IsLoaded And Not IsCreated Then
                RunXPreCheck = False
                Exit Function
            End If

            ' set msglog
            If MSGLOG Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                MSGLOG = _msglog
                MSGLOG.Create(Me.msglogtag)
            End If

            ' suspend Overloading
            If SUSPENDOVERLOAD Then Call SuspendOverloading(True)


            '* go through each object
            For Each anObject In Me.ObjectsByOrderNo

                ' special handling for special objects
                Select Case LCase(anObject.Objectname)

                    ' currtargets
                    Case "tblcurrtargets"
                        flag = True

                        ' currschedules
                    Case "tblcurrschedules"
                        flag = True

                        ' schedules
                    Case "tblschedules"
                        flag = True

                        ' HACK: CARTYPES
                    Case "tblconfigs"
                        flag = True

                        ' Targets
                    Case "tbldeliverabletargets"
                        flag = aTarget.runXPreCheckOLD(aMapping, Me, MSGLOG)
                        '
                    Case Else
                        ' default
                        flag = runDefaultXPreCheck(anObject, aMapping, MSGLOG)
                End Select
            Next

            ' suspend Overloading
            If SUSPENDOVERLOAD Then Call SuspendOverloading(False)

            RunXPreCheck = flag
        End Function

        '***** runDefaultXPreCheck
        '*****
        Public Function runDefaultXPreCheck(ByRef anObject As clsOTDBXChangeMember,
        ByRef aMapping As Dictionary(Of Object, Object),
        ByRef MSGLOG As ObjectLog) As Boolean
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim PkFields As Collection
            Dim aConfigmember As clsOTDBXChangeMember
            Dim aFieldDef As ObjectEntryDefinition
            Dim pkarr() As Object
            Dim m As Object
            Dim Value As Object
            Dim flag As Boolean
            Dim aDefTable As ObjectDefinition

            If Not IsLoaded And Not IsCreated Then
                runDefaultXPreCheck = False
                Exit Function
            End If

            ' set msglog
            If MSGLOG Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                MSGLOG = _msglog
                MSGLOG.Create(Me.msglogtag)
            End If

            '* get the Table
            aDefTable = anObject.[ObjectDefinition]
            If aDefTable Is Nothing Then
                WriteLine("???")
                Call CoreMessageHandler(arg1:=anObject.Objectname,
                                      subname:="clsOTDBXChangeConfig.runDEfaultXpreCheck",
                                      message:=" Table with name could not be loaded")
                runDefaultXPreCheck = False
                Exit Function
            End If

            flag = True
            ' run through the definition and get the mapped value -> check on it
            For Each m In aDefTable.Entries
                aFieldDef = m
                If aFieldDef.ID <> "" Then

                    aConfigmember = Me.AttributeByFieldName(aFieldDef.Name, tablename:=aFieldDef.Objectname)
                    If Not aConfigmember Is Nothing Then
                        If aMapping.ContainsKey(key:=aConfigmember.ordinal.Value) Then
                            Value = aMapping.Item(key:=aConfigmember.ordinal.Value)
                            ' check with the help of the FieldDef
                            If Not IsEmpty(Value) Then
                                flag = flag And aFieldDef.CheckOnFieldType(Value, MSGLOG)
                            End If
                        End If
                    End If
                End If
            Next m

            '** say checking is ok
            If flag Then
                Call MSGLOG.AddMsg("191", Nothing, Nothing, aFieldDef.ID, aFieldDef.Datatype)
            End If
            runDefaultXPreCheck = flag
        End Function

        '**** getMemberValue returns the Value of the Mapping of a ConfigMember
        '****
        Public Function GetMemberValue(Optional ByVal ID As String = "",
                                        Optional ByRef changemember As clsOTDBXChangeMember = Nothing,
                                        Optional ByRef tableentry As ObjectEntryDefinition = Nothing,
                                        Optional ByRef mapping As Dictionary(Of Object, Object) = Nothing,
                                        Optional ByVal objectname As String = ""
                                        ) As Object
            Dim aChangeMember As clsOTDBXChangeMember
            Dim outvalue As Object
            Dim aliases() As String = New String() {}
            Dim IDs() As String = New String() {}
            Dim anID As Object

            '**** check the input
            '****
            If mapping.Count = 0 Then
                Call CoreMessageHandler(message:="MAPPING is Nothing - not optional", subname:="clsOTDBXChangeConfig.getMemberValue")
                GetMemberValue = Null()
                Exit Function
            End If

            If ID = "" And changemember Is Nothing And tableentry Is Nothing Then
                Call CoreMessageHandler(message:="Neither ID, TABLEENRY nor CHANGEMEMBER provided", subname:="clsOTDBXChangeConfig.getMemberValue")
                GetMemberValue = Null()
                Exit Function
            End If

            '*** build the array
            '***
            If Trim(ID) <> "" Then
                IDs = IDs.Union(New String() {ID}).ToArray
                'Call ConcatenateArrays(IDs, Array(ID))
            ElseIf Not changemember Is Nothing Then
                IDs = IDs.Union(New String() {changemember.ID}).ToArray
                'Call ConcatenateArrays(IDs, Array(CHANGEMEMBER.ID))
            ElseIf Not tableentry Is Nothing Then
                IDs = IDs.Union(New String() {tableentry.ID}).ToArray
                'Call ConcatenateArrays(IDs, Array(TABLEENTRY.ID))
            End If
            '** add aliases
            If Not changemember Is Nothing Then
                aliases = changemember.Aliases
                IDs = IDs.Union(aliases).ToArray
                'If Not ConcatenateArrays(IDs, aliases) Then
                ' Debug.Assert False
                'End If
            ElseIf Not tableentry Is Nothing Then
                aliases = tableentry.Aliases
                IDs = IDs.Union(aliases).ToArray
                'If Not ConcatenateArrays(IDs, aliases) Then
                '   Debug.Assert False
                'End If
            End If

            '***
            '*** search for IDs and aliases in XCHANGECONFIG and get the value out of the MAPPING
            '*** convert it to DB Format and return
            '***
            For Each anID In IDs
                aChangeMember = Me.AttributeByID(ID:=anID, objectname:=objectname)
                If aChangeMember Is Nothing Then
                    For Each anEntryDef As ObjectEntryDefinition In CurrentSession.Objects.GetEntryByID(id:=anID, objectname:=objectname)
                        For Each aliasid In anEntryDef.Aliases
                            aChangeMember = Me.AttributeByAlias(alias:=aliasid, objectname:=objectname)
                            '* found
                            If Not aChangeMember Is Nothing AndAlso mapping.ContainsKey(aChangeMember.ordinal.Value) Then
                                Exit For
                            End If
                        Next
                        '* found
                        If Not aChangeMember Is Nothing Then
                            Exit For
                        End If
                    Next
                End If
                '** look up value
                If Not aChangeMember Is Nothing Then
                    If mapping.ContainsKey(aChangeMember.ordinal.Value) Then
                        outvalue = mapping.Item(aChangeMember.ordinal.Value)
                        'Call aChangeMember.convertValue2DB(outvalue, getMemberValue, existingValue:=False)
                        ' donot transfer to DB-Value (Mapping in Place ?!)
                        GetMemberValue = outvalue
                        Exit Function
                    Else
                        outvalue = Null()
                    End If
                Else
                    outvalue = Null()
                End If
            Next anID

            ' return the null
            GetMemberValue = outvalue
        End Function
        '**** updateValues
        '****
        Public Function updateRecordValues(ByRef ObjectDef As clsOTDBXChangeMember,
        ByRef MAPPING As Dictionary(Of Object, Object),
        ByRef MSGLOG As ObjectLog,
        ByRef RECORD As ormRecord) As Boolean
            '*** now we copy the object
            Dim aDefTable As New ObjectDefinition
            Dim aFieldDef As ObjectEntryDefinition
            Dim aConfigmember As clsOTDBXChangeMember
            Dim m As Object
            Dim aVAlue As Object
            Dim persistflag As Boolean
            Dim outvalue As Object

            ' set msglog
            If MSGLOG Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                MSGLOG = _msglog
                MSGLOG.Create(Me.msglogtag)
            End If

            aDefTable = ObjectDef.[ObjectDefinition]
            ' go through the table and overwrite the Record if the rights are there
            For Each m In aDefTable.Entries
                aFieldDef = m
                If aFieldDef.ID <> "" Then
                    aConfigmember = Me.AttributeByFieldName(aFieldDef.Name, tablename:=aFieldDef.Objectname)
                    If Not aConfigmember Is Nothing Then
                        ' we have a value AND update !
                        If MAPPING.ContainsKey(key:=aConfigmember.ordinal.Value) And
                        (aConfigmember.XChangeCmd = otXChangeCommandType.Update Or aConfigmember.XChangeCmd = otXChangeCommandType.UpdateCreate) Then
                            aVAlue = MAPPING.Item(key:=aConfigmember.ordinal.Value)
                            ' ** convert
                            If aConfigmember.convertValue2DB(inValue:=aVAlue, outvalue:=outvalue, MSGLOG:=MSGLOG) Then
                                persistflag = True
                                '** set value
                                If aFieldDef.Name <> ConstFNCreatedOn And aFieldDef.Name <> ConstFNUpdatedOn Then
                                    '* set the record
                                    Call RECORD.SetValue(aFieldDef.Name, outvalue)
                                End If
                            Else
                                '*** problem > msglog
                            End If
                        End If
                    End If
                End If
            Next m
            ' save it
            If persistflag Then
                updateRecordValues = RECORD.Persist
            Else
                updateRecordValues = False
            End If
        End Function

#Region "Static functions"

        '****** AllByList: "static" function to return all Objects
        '******
        Public Function AllByList() As List(Of clsOTDBXChangeConfig)
            Dim aList As New List(Of clsOTDBXChangeConfig)
            Dim aRecordCollection As List(Of ormRecord)
            Dim aTable As iormDataStore

            Dim aRecord As ormRecord

            Try
                aTable = GetTableStore(constTableID)
                aRecordCollection = aTable.GetRecordsBySql(wherestr:="configname <> ''")

                If aRecordCollection Is Nothing Then
                    Me.Unload()
                    AllByList = Nothing
                    Exit Function
                Else
                    For Each aRecord In aRecordCollection
                        Dim aNewObject As New clsOTDBXChangeConfig
                        aNewObject = New clsOTDBXChangeConfig
                        If aNewObject.Infuse(aRecord) Then
                            ' loadby to get all items
                            If aNewObject.LoadBy(aNewObject.Configname) Then
                                aList.Add(item:=aNewObject)
                            End If
                        End If
                    Next aRecord
                    AllByList = aList
                    Exit Function
                End If

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="clsOTDBXChangeConfig.AllByList")
                Return aList
            End Try
        End Function
#End Region
    End Class



    ''' <summary>
    ''' describes a XChange Member - an individual 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class clsOTDBXChangeMember
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        '***
        '*** Meta Definition PErsistency
        <ormSchemaTableAttribute(version:=3)> Public Const ConstTableID = "tblXChangeConfigMembers"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=50, primaryKeyordinal:=1,
            title:="XChangeConfigID", description:="name of the XchangeConfiguration")>
        Public Const ConstFNID = "configname"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Long, primaryKeyordinal:=2,
           title:="IndexNo", description:="position in the the XchangeConfiguration")>
        Public Const ConstFNIDNo = "idno"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=100,
           title:="ObjectName", description:="Name of the ObjectDefinition")>
        Public Const constFNObjectname = "objectname"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=100,
          title:="EntryName", description:="Name of the Entry in theObjectDefinition")>
        Public Const constFNEntryname = "entryname"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=255,
         title:="Description", description:="Description of the Entry")>
        Public Const constFNDesc = "desc"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Memo, title:="Comment", description:="Comment")>
        Public Const constFNComment = "cmt"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=50,
         title:="XChange ID", description:="ID  of the Attribute in theObjectDefinition")>
        Public Const constFNXID = "id"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=250,
        title:="Parameter", description:="Parameter for the Attribute")>
        Public Const constFNParameter = "parameter"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=250,
        title:="ordinal", description:="ordinal for the Attribute Mapping")>
        Public Const constFNordinal = "ordinal"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=250,
       title:="Relation", description:="Relation for the Attribute")>
        Public Const constFNRelation = "relation"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, title:="Is Object Entry", description:="Set if this is an object entry")>
        Public Const constFNIsObjectEntry = "isobj"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, title:="Is Attribute Entry", description:="Set if this is an compound entry")>
        Public Const constFNIsAttributeEntry = "isattr"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, title:="Is Compound Entry", description:="Set if this is an compound entry")>
        Public Const constFNIsCompoundEntry = "iscomp"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, title:="Is Entry Read-Only", description:="Set if this entry is read-only - value in OTDB cannot be overwritten")>
        Public Const constFNIsReadonly = "isro"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, title:="Is ordered", description:="Set if this entry is ordered")>
        Public Const constFNIsOrder = "isorder"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, title:="Is dynamic attribute", description:="Set if this entry is dynamic")>
        Public Const constFNIsDynamic = "isdynamic"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, title:="Attribute is not exchanged", description:="Set if this attribute is not exchanged")>
        Public Const constFNIsNotXChanged = "isnxchg"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.List, size:=50, parameter:="parameter_xcmd_list",
            title:="XChange Command", description:="XChangeCommand to run on this")>
        Public Const constFNXCMD = "xcmd"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Long, title:="Order Number", description:="Order number in which entriy is processed")>
        Public Const constFNOrderNo = "orderno"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=250, title:="MessageLogTag", description:="Message Log Tag")>
        Public Const constFNMsgLogTag = "msglogtag"


        ' fields
        <ormColumnMappingAttribute(fieldname:=constFNID)> Private _configname As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNIDNo)> Private _idno As Long
        <ormColumnMappingAttribute(fieldname:=constFNXID)> Private _xid As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNObjectname)> Private _objectname As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNEntryname)> Private _entryname As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNRelation)> Private _relation As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNParameter)> Private _parameter As String = ""
        '<otColumnMapping(fieldname:=constFNordinal)> do not since we cannot map it
        Private _ordinal As Ordinal = New Ordinal(0)
        <ormColumnMappingAttribute(fieldname:=constFNComment)> Private _cmt As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNDesc)> Private _desc As String = ""
        <ormColumnMappingAttribute(fieldname:=constFNIsNotXChanged)> Private _isNotXChanged As Boolean
        <ormColumnMappingAttribute(fieldname:=constFNIsReadonly)> Private _isReadOnly As Boolean
        <ormColumnMappingAttribute(fieldname:=constFNIsAttributeEntry)> Private _isAttributeEntry As Boolean
        <ormColumnMappingAttribute(fieldname:=constFNIsObjectEntry)> Private _isObjectEntry As Boolean
        <ormColumnMappingAttribute(fieldname:=constFNIsCompoundEntry)> Private _isCompundEntry As Boolean
        <ormColumnMappingAttribute(fieldname:=constFNXCMD)> Private _xcmd As otXChangeCommandType
        <ormColumnMappingAttribute(fieldname:=constFNIsOrder)> Private _isOrdered As Boolean
        <ormColumnMappingAttribute(fieldname:=constFNOrderNo)> Private _orderNo As Long
        <ormColumnMappingAttribute(fieldname:=constFNIsDynamic)> Private _isDynamicAttribute As Boolean

        'dynamic
        Private _EntryDefinition As ObjectEntryDefinition
        Private _ObjectDefinition As ObjectDefinition
        Private _aliases As String()    ' not saved !
        Private _msglog As New ObjectLog
        Private _msglogtag As String

        '** initialize
        Public Sub New()
            Call MyBase.New(constTableID)

            _EntryDefinition = Nothing
        End Sub

#Region "Properties"

        ''' <summary>
        ''' Gets the S is compund entry.
        ''' </summary>
        ''' <value>The S is compund entry.</value>
        Public ReadOnly Property IsCompundEntry() As Boolean
            Get
                Return Me._isCompundEntry
            End Get
        End Property

        ''' <summary>
        ''' gets or sets the objectname
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Objectname() As String
            Get
                Objectname = _objectname
            End Get
            Set(value As String)
                If LCase(_objectname) <> LCase(value) Then
                    _objectname = LCase(value)
                    Me.IsChanged = True
                End If
            End Set
        End Property

        '****** getUniqueTag
        Public Function GetUniqueTag()
            GetUniqueTag = ConstDelimiter & constTableID & ConstDelimiter & _configname & ConstDelimiter & _xid & ConstDelimiter & _objectname & ConstDelimiter & _entryname & ConstDelimiter
        End Function
        ''' <summary>
        ''' gets the MSGLog Tag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Msglogtag() As String
            Get
                If _msglogtag = "" Then
                    _msglogtag = GetUniqueTag()
                End If
                Msglogtag = _msglogtag
            End Get

        End Property

        ''' <summary>
        ''' gets or sets the XChange ID for the Member
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ID() As String
            Get
                ID = _xid
            End Get
            Set(avalue As String)

                If LCase(_xid) <> LCase(avalue) Then
                    _xid = avalue
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' gets the fieldname
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Entryname() As String
            Get
                Entryname = _entryname
            End Get
            Set(avalue As String)
                If LCase(_entryname) <> LCase(avalue) Then
                    _entryname = avalue
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' gets the configname
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Configname() As String
            Get
                Configname = _configname
            End Get
            Set(value As String)
                If LCase(_configname) <> LCase(value) Then
                    _configname = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' gets the Aliases of the XChange Member
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Aliases() As String()
            Get
                If Not Me.ObjectEntryDefinition Is Nothing Then
                    Aliases = _EntryDefinition.Aliases
                Else
                    Aliases = New String() {}
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
        ReadOnly Property HasAlias([alias] As String) As Boolean
            Get
                If Not Me.ObjectEntryDefinition Is Nothing Then
                    _aliases = _EntryDefinition.Aliases
                Else
                    Return False
                End If
                Return _aliases.Contains(UCase([alias]))

            End Get
        End Property

        ''' <summary>
        ''' gets or sets the Xchange Command
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property XChangeCmd() As otXChangeCommandType
            Get
                XChangeCmd = _xcmd
            End Get
            Set(value As otXChangeCommandType)
                If _xcmd <> value Then
                    _xcmd = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets the ObjectEntry Definition for the XChange Member
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property [ObjectEntryDefinition] As ObjectEntryDefinition
            Get
                Dim anEntryDefinition As ObjectEntryDefinition
                If (Me.IsCreated Or Me.IsLoaded) And Me.IsAttributeEntry And _EntryDefinition Is Nothing Then

                    If Me.Entryname <> "" And Me.Objectname <> "" Then
                        anEntryDefinition = CurrentSession.Objects.GetEntry(objectname:=Me.Objectname, entryname:=Me.Entryname)
                    ElseIf Me.Objectname <> "" And Me.ID <> "" Then
                        anEntryDefinition = CurrentSession.Objects.GetEntryByID(id:=Me.ID, objectname:=Me.Objectname).First
                    Else
                        anEntryDefinition = CurrentSession.Objects.GetEntryByID(id:=Me.ID).First
                    End If
                    If Not anEntryDefinition Is Nothing Then
                        _EntryDefinition = anEntryDefinition
                    End If
                End If

                Return _EntryDefinition
                ' return
                [ObjectEntryDefinition] = Nothing
            End Get
            Set(value As ObjectEntryDefinition)
                _EntryDefinition = value
            End Set
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
        Public Property ordinal() As Ordinal
            Get
                ordinal = _ordinal
            End Get
            Set(value As Ordinal)
                _ordinal = value
                Me.IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' gets or sets parameter
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Parameter() As String
            Get
                Parameter = _parameter
            End Get
            Set(value As String)
                If LCase(_parameter) <> LCase(value) Then
                    _parameter = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets relation
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Relation() As Object
            Get
                Relation = SplitMultiDelims(text:=_relation, DelimChars:=ConstDelimiter)
            End Get
            Set(avalue As Object)
                Dim i As Integer
                If IsArrayInitialized(avalue) Then
                    Dim aStrValue As String
                    For i = LBound(avalue) To UBound(avalue)
                        If i = LBound(avalue) Then
                            aStrValue = avalue(i)
                        Else
                            aStrValue = aStrValue & ConstDelimiter & avalue(i)
                        End If
                    Next i
                    _relation = aStrValue
                    Me.IsChanged = True
                ElseIf Not IsEmpty(Trim(avalue)) And Trim(avalue) <> "" And Not IsNull(avalue) Then
                    _relation = CStr(Trim(avalue))
                Else
                    _relation = ""
                End If
            End Set
        End Property

        ''' <summary>
        ''' gets or sets comment
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Comment() As String
            Get
                Comment = _cmt
            End Get
            Set(value As String)
                If _cmt <> value Then
                    _cmt = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' Primary Key Indexno
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Indexno() As Long
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
        Public Property IsXChanged() As Boolean
            Get
                IsXChanged = Not _isNotXChanged
            End Get
            Set(value As Boolean)
                If _isNotXChanged <> Not value Then
                    _isNotXChanged = Not value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' sets the Readonly Flag - value of the OTDB cannot be overwritten
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsReadOnly() As Boolean
            Get
                IsReadOnly = _isReadOnly
            End Get
            Set(value As Boolean)
                If _isReadOnly <> value Then
                    _isReadOnly = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the Attribute Entry Flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsAttributeEntry() As Boolean
            Get
                IsAttributeEntry = _isAttributeEntry

            End Get
            Set(value As Boolean)
                If _isAttributeEntry <> value Then
                    _isAttributeEntry = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets True if this is a Compound
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsCompound() As Boolean
            Get
                Dim aFieldDef As ObjectEntryDefinition
                If Me.IsAttributeEntry Then
                    aFieldDef = Me.ObjectEntryDefinition
                    If Not aFieldDef Is Nothing Then
                        IsCompound = aFieldDef.IsCompound
                        Exit Property
                    End If
                End If
                IsCompound = False
            End Get

        End Property
        ''' <summary>
        ''' gets True if the Attribute is a Field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsField() As Boolean
            Get
                Dim aFieldDef As ObjectEntryDefinition
                If Me.IsAttributeEntry Then
                    aFieldDef = Me.[ObjectEntryDefinition]
                    If Not aFieldDef Is Nothing Then
                        IsField = aFieldDef.IsField
                        Exit Property
                    End If
                End If
                IsField = False
            End Get
        End Property

        ''' <summary>
        ''' gets True if this is an Object Entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsObjectEntry() As Boolean
            Get
                IsObjectEntry = _isObjectEntry
            End Get
            Set(value As Boolean)
                If _isObjectEntry <> value Then
                    _isObjectEntry = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the OrderedBy Flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsOrderedBy() As Boolean
            Get
                IsOrderedBy = _isOrdered
            End Get
            Set(value As Boolean)
                If _isOrdered <> value Then
                    _isOrdered = value
                    Me.IsChanged = True
                End If
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
                Orderno = _orderNo
            End Get
            Set(value As Long)
                _orderNo = value
                Me.IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the Dynamic Flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsDynamicAttribute() As Boolean
            Get
                IsDynamicAttribute = _isDynamicAttribute And _isAttributeEntry
            End Get
            Set(value As Boolean)
                If _isDynamicAttribute <> value And _isAttributeEntry Then
                    _isDynamicAttribute = value
                    Me.IsChanged = True
                End If
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
                Call CoreMessageHandler(subname:="clsOTDBXchangeMember.incordinal", message:="ordinal is not numeric")
                Incordinal = Nothing
                Exit Function
            End If
            Incordinal = _ordinal
        End Function
        '**** set the values by FieldDesc
        '****
        ''' <summary>
        ''' sets the XChange Member to the values of a FieldDescription
        ''' </summary>
        ''' <param name="aFieldDesc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetByFieldDesc(fielddesc As ormFieldDescription) As Boolean
            If Not _IsLoaded And Not Me.IsCreated Then
                SetByFieldDesc = False
                Exit Function
            End If

            If fielddesc.ID <> "" Then
                _xid = fielddesc.ID
                _aliases = fielddesc.Aliases
            Else
                Me.Entryname = fielddesc.ColumnName
            End If
            Me.Objectname = fielddesc.Tablename

            Return Me.IsChanged
        End Function
        ''' <summary>
        ''' infuses the XChange member from the record
        ''' </summary>
        ''' <param name="record"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean
            Dim aValue As Object

            Try
                If MyBase.Infuse(record) Then
                    If IsNull(record.GetValue(constFNordinal)) Then
                        _ordinal = New Ordinal(0)
                    Else
                        aValue = record.GetValue(constFNordinal)
                        If IsNumeric(aValue) Then
                            _ordinal = New Ordinal(CLng(aValue))
                        Else
                            _ordinal = New Ordinal(CStr(aValue))
                        End If
                    End If
                End If

                Return Me.IsLoaded
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="clsOTDBXChangeMember.Infuse")
                Unload()
                Return False
            End Try

        End Function

        ''' <summary>
        ''' Load XChange Member from persistence store
        ''' </summary>
        ''' <param name="configname"></param>
        ''' <param name="indexno"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal configname As String, ByVal indexno As Long) As Boolean
            Dim pkarry() As Object = {LCase(configname), indexno}
            Return MyBase.LoadBy(pkarry)
        End Function
        ''' <summary>
        ''' Create Persistence Schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean

            Return ormDataObject.CreateSchema(Of clsOTDBXChangeMember)()


            Dim aFieldDesc As New ormFieldDescription
            Dim PrimaryColumnNames As New Collection
            Dim aTable As New ObjectDefinition


            aFieldDesc.ID = ""
            aFieldDesc.Parameter = ""
            aFieldDesc.Tablename = constTableID

            With aTable
                .Create(constTableID)
                .Delete()

                '***
                '*** Fields
                '****

                'Type
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = constFNID
                aFieldDesc.ColumnName = constFNID
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                'index pos
                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "posno in index (primary key)"
                aFieldDesc.ColumnName = constFNIDNo
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                'objectname
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "objectname"
                aFieldDesc.ColumnName = "objectname"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)


                'Fieldnames
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "fieldname"
                aFieldDesc.ColumnName = "fieldname"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                'objectname
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "description"
                aFieldDesc.ColumnName = "desc"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                'objectname
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "comment"
                aFieldDesc.ColumnName = "cmt"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)


                ' id
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "xChange id"
                aFieldDesc.ColumnName = "id"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                'title
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "title"
                aFieldDesc.ColumnName = "title"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                'Parameter
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "Parameter"
                aFieldDesc.ColumnName = "parameter"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                'ordinal
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "ordinal key for mapping"
                aFieldDesc.ColumnName = "ordinal"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                'Relation
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "Relation"
                aFieldDesc.ColumnName = "relation"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                '
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is an ObjectEntry"
                aFieldDesc.ColumnName = "isobj"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                '
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is an Attribute"
                aFieldDesc.ColumnName = "isattr"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                'aFieldDesc.dataType = OTDBFieldDataType.bool
                'aFieldDesc.title = "is a Compound"
                'aFieldDesc.Name = "iscomp"
                'Call .addFieldDesc(FIELDDESC:=aFieldDesc)

                '
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is read only"
                aFieldDesc.ColumnName = "isro"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                'dynamic
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is dynamic attribute"
                aFieldDesc.ColumnName = "isdynamic"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                '
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is order field"
                aFieldDesc.ColumnName = "isorder"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is not exchanged(temp)"
                aFieldDesc.ColumnName = "isnxchg"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                '
                aFieldDesc.Datatype = otFieldDataType.List
                aFieldDesc.Title = "xchange command"
                aFieldDesc.ColumnName = "xcmd"
                aFieldDesc.Parameter = "parameter_xcmd_list"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                aFieldDesc.Parameter = ""

                'sort order
                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "order by seq no."
                aFieldDesc.ColumnName = "orderno"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' msglogtag
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "message log tag"
                aFieldDesc.ColumnName = "msglogtag"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.ColumnName = ConstFNCreatedOn
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' Index
                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
                ' persist
                .Persist()
                ' change the database
                .AlterSchema()
            End With

            CreateSchema = True
            Exit Function

            ' Handle the error
error_handle:
            Call CoreMessageHandler(subname:="clsOTDBXChangeMember.createSchema")
            CreateSchema = False
        End Function
        ''' <summary>
        ''' Persist the Xchange Member
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean
            Try
                '* ordinal
                If Me.ordinal = New Ordinal(0) And Orderno <> 0 Then
                    Me.ordinal = New Ordinal(Orderno)
                End If
                If Orderno = 0 And Me.ordinal <> New Ordinal(0) And ordinal.Type = ordinalType.longType Then
                    Me.Orderno = Me.ordinal.Value
                End If
                Call Me.Record.SetValue(constFNordinal, _ordinal.Value.ToString)
                Return MyBase.Persist(timestamp, doFeedRecord:=True)
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="clsOTDBXChangemember.Persist")
                Return False
            End Try
        End Function
        ''' <summary>
        ''' creates a persistable XChange member with primary Key
        ''' </summary>
        ''' <param name="configname"></param>
        ''' <param name="indexno"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal configname As String, Optional ByVal indexno As Long = 0) As Boolean
            Dim pkarray() As Object = {LCase(configname), indexno}
            If MyBase.Create(pkArray:=pkarray, checkUnique:=False) Then
                ' set the primaryKey
                _configname = LCase(configname)
                _idno = indexno
                Return Me.IsCreated
            Else
                Return False
            End If
        End Function


        '**** convertValue4DB : Checks the given inValue according to the datatype
        '****
        Public Function convertValue4DB(ByVal inValue As Object,
        ByRef outvalue As Object,
        Optional ByRef MSGLOG As ObjectLog = Nothing) As Boolean

            Dim aFieldDef As New ObjectEntryDefinition
            Dim result As Object
            Dim index As Integer

            aFieldDef = Me.ObjectEntryDefinition
            If aFieldDef Is Nothing Then
                convertValue4DB = False
                Exit Function
            End If

            ' set msglog
            If MSGLOG Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                MSGLOG = _msglog
                MSGLOG.Create(Me.Msglogtag)
            End If

            '** check on Error from Formular
            If IsError(inValue) Then
                inValue = Nothing
            End If

            convertValue4DB = True
            '*** transfer
            '****
            Select Case aFieldDef.Datatype
                Case otFieldDataType.[Long]
                    If (IsEmpty(outvalue) OrElse IsNull(outvalue)) AndAlso (Not IsNumeric(inValue)) Then
                        outvalue = 0
                    ElseIf Not IsNumeric(inValue) And (Not IsEmpty(outvalue) And Not IsNull(outvalue)) Then
                        outvalue = outvalue    ' simply keep it
                    ElseIf IsNumeric(inValue) Then
                        outvalue = CLng(inValue)
                    Else
                        Call CoreMessageHandler(subname:="clsOTDBXChangeMember.convertValue4DB", entryname:=aFieldDef.name,
                                              message:="OTDB data " & inValue & " is not convertible to long",
                                              arg1:=inValue)
                        convertValue4DB = False
                        outvalue = Null()
                    End If
                Case otFieldDataType.Numeric
                    If Not IsNumeric(inValue) AndAlso (IsEmpty(outvalue) OrElse IsNull(outvalue)) Then
                        outvalue = Nothing
                    ElseIf Not IsNumeric(inValue) AndAlso IsNumeric(outvalue) Then
                        outvalue = outvalue    ' simply keep it
                    ElseIf IsNumeric(inValue) Then
                        outvalue = CDbl(inValue)
                    Else
                        Call CoreMessageHandler(subname:="clsOTDBXChangeMember.convertValue4DB", entryname:=aFieldDef.name,
                                              message:="OTDB data " & inValue & " is not convertible to Double",
                                              arg1:=inValue)
                        WriteLine("OTDB data " & inValue & " is not convertible to Integer")

                        outvalue = Null()
                        convertValue4DB = False
                    End If

                Case otFieldDataType.Text, otFieldDataType.List, otFieldDataType.Memo
                    If (IsEmpty(outvalue) OrElse IsNull(outvalue)) AndAlso (IsNull(inValue) OrElse inValue = "-") Then
                        outvalue = ""
                    ElseIf (Trim(inValue) = "" OrElse IsEmpty(inValue)) AndAlso
                    (Not IsEmpty(outvalue) AndAlso Not IsNull(outvalue)) Then
                        outvalue = outvalue    ' simply keep it
                    Else
                        outvalue = CStr(inValue)
                        'Else
                        '    Call OTDBErrorHandler(subname:="clsOTDBXChangeMember.convertValue4DB", entryname:=aFieldDef.fieldname, 
                        '                          message:="OTDB data " & inValue & " is not convertible to Double", 
                        '                          arg1:=Value)
                        '    writeline( "OTDB data " & inValue & " is not convertible to Integer"
                        '    outValue = Null
                    End If
                Case otFieldDataType.Runtime
                    Call CoreMessageHandler(subname:="clsOTDBXChangeMember.convertValue4DB", entryname:=aFieldDef.name,
                                          message:="OTDB data " & inValue & " is not convertible from/to runtime",
                                          arg1:=inValue)

                    outvalue = Null()
                    convertValue4DB = False
                Case otFieldDataType.Formula
                    Call CoreMessageHandler(subname:="clsOTDBXChangeMember.convertValue4DB", entryname:=aFieldDef.name,
                                          message:="OTDB data " & inValue & " is not convertible from/to formula",
                                          arg1:=inValue)

                    outvalue = Null()
                    convertValue4DB = False
                Case otFieldDataType.[Date], otFieldDataType.Time, otFieldDataType.Timestamp

                    If (IsEmpty(outvalue) OrElse IsNull(outvalue)) AndAlso (Not IsDate(inValue)) Then
                        outvalue = Nothing
                    ElseIf (IsEmpty(inValue)) AndAlso IsDate(outvalue) Then
                        outvalue = outvalue    ' simply keep it
                    ElseIf IsDate(inValue) AndAlso inValue = ConstNullDate Then
                        outvalue = "-"
                    ElseIf IsDate(inValue) AndAlso inValue <> ConstNullDate Then
                        outvalue = inValue
                    Else
                        outvalue = Nothing
                        Call CoreMessageHandler(subname:="clsOTDBXChangeMember.convertValue4DB", entryname:=aFieldDef.name,
                                              message:="OTDB data " & inValue & " is not convertible to Date",
                                              arg1:=inValue)


                        convertValue4DB = False
                    End If


                Case otFieldDataType.Bool
                    If inValue = True OrElse (Not IsEmpty(inValue) AndAlso Not IsNull(inValue)) Then
                        outvalue = True
                    Else
                        outvalue = False
                        'Else
                        '    Call OTDBErrorHandler(subname:="clsOTDBXChangeMember.convertValue4DB", entryname:=aFieldDef.fieldname, 
                        '                          message:="OTDB data " & inValue & " is not convertible to Double", 
                        '                          arg1:=Value)
                        '    writeline( "OTDB data " & inValue & " is not convertible to Integer"
                        '    outValue = Null
                    End If

                Case otFieldDataType.Binary
                    outvalue = inValue
            End Select


        End Function

        '**** convertValue2DB : Checks the given inValue according to the datatype
        '****
        '**** -> value of datatype or Nothing is allowed as output
        '****
        Public Function convertValue2DB(ByVal inValue As Object,
        ByRef outvalue As Object,
        Optional ByVal existingValue As Boolean = True,
        Optional ByRef MSGLOG As ObjectLog = Nothing) As Boolean

            Dim aFieldDef As New ObjectEntryDefinition
            Dim result As Object
            Dim index As Integer

            aFieldDef = Me.ObjectEntryDefinition
            If aFieldDef Is Nothing Then
                convertValue2DB = False
                Exit Function
            End If
            ' set msglog
            If MSGLOG Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                MSGLOG = _msglog
                MSGLOG.Create(Me.Msglogtag)
            End If
            '** check on Error from Formular
            If IsError(inValue) Then
                inValue = Nothing
            End If

            convertValue2DB = True
            '*** transfer
            '****
            Select Case aFieldDef.Datatype

                Case otFieldDataType.Numeric, otFieldDataType.[Long]
                    If (inValue Is Nothing And Not existingValue) Then
                        outvalue = Nothing
                    ElseIf inValue Is Nothing And existingValue Then
                        If aFieldDef.Datatype = otFieldDataType.Numeric Then
                            outvalue = CDbl(outvalue)    ' simply keep it
                        Else
                            outvalue = CLng(outvalue)
                        End If
                    ElseIf DBNull.Value.Equals(inValue) Or Trim(inValue.ToString) = "-" Or IsEmpty(inValue) Then
                        outvalue = 0
                    ElseIf (Trim(inValue.ToString) = "" Or IsError(inValue)) And (existingValue And IsNumeric(outvalue)) Then
                        If aFieldDef.Datatype = otFieldDataType.Numeric Then
                            outvalue = CDbl(outvalue)    ' simply keep it
                        Else
                            outvalue = CLng(outvalue)
                        End If
                    ElseIf IsNumeric(inValue) Then
                        If aFieldDef.Datatype = otFieldDataType.Numeric Then
                            outvalue = CDbl(inValue)    ' simply keep it
                        Else
                            outvalue = CLng(inValue)
                        End If
                    Else
                        Call CoreMessageHandler(subname:="clsOTDBXChangeMember.convertValue2DB", entryname:=aFieldDef.name,
                                              message:="OTDB data " & inValue & " is not convertible to Double",
                                              arg1:=inValue)
                        WriteLine("OTDB data " & inValue & " is not convertible to Integer")
                        outvalue = Null()
                        convertValue2DB = False
                    End If

                Case otFieldDataType.Text, otFieldDataType.List, otFieldDataType.Memo
                    If (inValue Is Nothing And Not existingValue) Then
                        outvalue = Nothing
                    ElseIf (inValue Is Nothing OrElse Trim(inValue) = "" OrElse IsError(inValue) OrElse IsEmpty(inValue)) _
                        AndAlso existingValue Then
                        ' nothing
                    ElseIf IsNull(inValue) Or Trim(inValue) = "-" Or (Trim(inValue) = "" And Not existingValue) Then
                        outvalue = ""
                    Else
                        outvalue = CStr(inValue)
                        'Else
                        '    Call OTDBErrorHandler(subname:="clsOTDBXChangeMember.convertValue2DB", entryname:=aFieldDef.fieldname, 
                        '                          message:="OTDB data " & inValue & " is not convertible to Double", 
                        '                          arg1:=Value)
                        '    writeline( "OTDB data " & inValue & " is not convertible to Integer"
                        '    outValue = Null
                    End If
                Case otFieldDataType.Runtime
                    Call CoreMessageHandler(subname:="clsOTDBXChangeMember.convertValue2DB", entryname:=aFieldDef.name,
                                          message:="OTDB data " & inValue & " is not convertible from/to runtime",
                                          arg1:=inValue)

                    outvalue = Null()
                    convertValue2DB = False
                Case otFieldDataType.Formula
                    Call CoreMessageHandler(subname:="clsOTDBXChangeMember.convertValue2DB", entryname:=aFieldDef.name,
                                          message:="OTDB data " & inValue & " is not convertible from/to formula",
                                          arg1:=inValue)

                    outvalue = Null()
                    convertValue2DB = False
                Case otFieldDataType.[Date], otFieldDataType.Time, otFieldDataType.Timestamp
                    If (inValue Is Nothing And Not existingValue) Then
                        outvalue = Nothing
                    ElseIf inValue Is Nothing And existingValue Then
                        ' nothing keep it
                    ElseIf IsNull(inValue) OrElse Trim(inValue.ToString) = "-" OrElse
                        (Trim(inValue.ToString) = "" AndAlso Not existingValue) Then
                        outvalue = ConstNullDate
                    ElseIf (Trim(inValue.ToString) = "" OrElse IsEmpty(inValue)) AndAlso (existingValue AndAlso IsDate(outvalue)) Then
                        outvalue = outvalue    ' simply keep it
                    ElseIf IsDate(inValue) Then
                        outvalue = CDate(inValue)
                    Else
                        Call CoreMessageHandler(subname:="clsOTDBXChangeMember.convertValue2DB", entryname:=aFieldDef.name,
                                              message:="OTDB data " & inValue & " is not convertible to Date",
                                              arg1:=inValue)

                        outvalue = ConstNullDate
                        convertValue2DB = False
                    End If

                Case otFieldDataType.Bool
                    If (inValue Is Nothing And Not existingValue) Then
                        outvalue = Nothing
                    ElseIf (inValue Is Nothing OrElse Trim(inValue) = "" OrElse IsError(inValue) OrElse IsEmpty(inValue)) _
                        AndAlso existingValue Then
                        ' nothing
                    ElseIf Trim(inValue.ToString) = "-" And (existingValue And IsEmpty(outvalue) Or IsNull(outvalue)) Then
                        outvalue = False
                    ElseIf Trim(inValue.ToString) = "" And (existingValue And Not IsEmpty(outvalue) And Not IsNull(outvalue)) Then
                        outvalue = outvalue    ' simply keep it
                    ElseIf Not IsEmpty(inValue) Or inValue = True Then
                        outvalue = True

                    Else
                        outvalue = False
                        'Else
                        '    Call OTDBErrorHandler(subname:="clsOTDBXChangeMember.convertValue2DB", entryname:=aFieldDef.fieldname, 
                        '                          message:="OTDB data " & inValue & " is not convertible to Double", 
                        '                          arg1:=Value)
                        '    writeline( "OTDB data " & inValue & " is not convertible to Integer"
                        '    outValue = Null
                    End If

                Case otFieldDataType.Binary
                    outvalue = inValue
            End Select

            convertValue2DB = True
        End Function

        '**** compareValues : Checks the given leftValue according to the datatype
        '****
        Public Function compareValues(ByVal LEFTVALUE As Object,
        ByVal RIGHTVALUE As Object,
        Optional ByRef MSGLOG As ObjectLog = Nothing) As Integer

            Dim aFieldDef As New ObjectEntryDefinition

            aFieldDef = Me.ObjectEntryDefinition
            If aFieldDef Is Nothing Then
                compareValues = False
                Exit Function
            End If
            ' set msglog
            If MSGLOG Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                MSGLOG = _msglog
                MSGLOG.Create(Me.Msglogtag)
            End If
            '** check on Error from Formular
            If IsError(LEFTVALUE) Then
                LEFTVALUE = Nothing
            End If

            compareValues = True
            '*** compare in DB values
            Call Me.convertValue2DB(LEFTVALUE, LEFTVALUE, MSGLOG:=MSGLOG, existingValue:=False)
            Call Me.convertValue2DB(RIGHTVALUE, RIGHTVALUE, MSGLOG:=MSGLOG, existingValue:=False)

            If LEFTVALUE = Nothing And RIGHTVALUE = Nothing Then
                compareValues = 0
                Exit Function
            ElseIf LEFTVALUE = Nothing Or RIGHTVALUE = Nothing Then
                compareValues = -1
                Exit Function
            End If

            '*** transfer
            '****
            Select Case aFieldDef.Datatype

                Case otFieldDataType.Numeric, otFieldDataType.[Long]
                    If LEFTVALUE < RIGHTVALUE Then
                        compareValues = -1
                    ElseIf LEFTVALUE = RIGHTVALUE Then
                        compareValues = 0
                    Else
                        compareValues = 1
                    End If
                    Exit Function
                Case otFieldDataType.Text, otFieldDataType.List, otFieldDataType.Memo
                    compareValues = StrComp(LEFTVALUE, RIGHTVALUE, vbTextCompare)
                    Exit Function
                Case otFieldDataType.Runtime
                    Call CoreMessageHandler(subname:="clsOTDBXChangeMember.compareValues", entryname:=aFieldDef.name,
                                          message:="OTDB data " & LEFTVALUE & " is not convertible from/to runtime",
                                          arg1:=LEFTVALUE)

                    compareValues = StrComp(CStr(LEFTVALUE), CStr(RIGHTVALUE), vbTextCompare)
                    Exit Function
                Case otFieldDataType.Formula
                    Call CoreMessageHandler(subname:="clsOTDBXChangeMember.compareValues", entryname:=aFieldDef.name,
                                          message:="OTDB data " & LEFTVALUE & " is not convertible from/to formula",
                                          arg1:=LEFTVALUE)

                    compareValues = StrComp(CStr(LEFTVALUE), CStr(RIGHTVALUE), vbTextCompare)
                    Exit Function
                Case otFieldDataType.[Date], otFieldDataType.Time, otFieldDataType.Timestamp
                    If LEFTVALUE < RIGHTVALUE Then
                        compareValues = -1
                    ElseIf LEFTVALUE = RIGHTVALUE Then
                        compareValues = 0
                    Else
                        compareValues = -1
                    End If
                    Exit Function

                Case otFieldDataType.Bool
                    If LEFTVALUE And Not RIGHTVALUE Then
                        compareValues = -1
                    ElseIf LEFTVALUE And RIGHTVALUE Then
                        compareValues = 0
                    Else
                        compareValues = -1
                    End If
                    Exit Function

                Case otFieldDataType.Binary
                    If Len(LEFTVALUE) < Len(RIGHTVALUE) Then
                        compareValues = -1
                    ElseIf Len(LEFTVALUE) = Len(RIGHTVALUE) Then
                        compareValues = 0
                    Else
                        compareValues = -1
                    End If
                    Exit Function
            End Select


        End Function
        '**** checkOnDataType : Checks the given aValue according to the datatype
        '****
        Public Function checkOnDatatype(ByVal VALUE As Object, Optional ByRef MSGLOG As ObjectLog = Nothing) As Boolean

            '** check ok if special characters with special functions
            If Trim(VALUE) = ConstXChangeClearFieldValue Or Trim(VALUE) = "" Or IsEmpty(VALUE) Then
                checkOnDatatype = True
                Exit Function
            End If
        End Function

        Public Function runXChange(ByRef MAPPING As Dictionary(Of Object, Object),
        ByRef VARIABLE As Object,
        Optional ByVal FORCE As Boolean = False,
        Optional ByRef oldValuesMap As Dictionary(Of Object, Object) = Nothing,
        Optional ByRef MSGLOG As ObjectLog = Nothing) As Boolean
            Dim aVAlue As Object
            Dim oldValue As Object

            runXChange = True
            '* look into old values
            If Not oldValuesMap Is Nothing Then
                If oldValuesMap.ContainsKey(key:=Me.ordinal.Value) Then
                    oldValue = oldValuesMap.Item(key:=Me.ordinal.Value)
                Else
                    oldValue = Nothing
                End If
            Else
                oldValue = Nothing
            End If

            ' look into new Values

            ' update the variable with the value in the mapping
            If Me.XChangeCmd = otXChangeCommandType.Update Or Me.XChangeCmd = otXChangeCommandType.UpdateCreate Then
                If MAPPING.ContainsKey(Me.ordinal.Value) Then
                    aVAlue = MAPPING.Item(Me.ordinal.Value)

                Else
                    aVAlue = Nothing
                End If
                ' only if diffrent
                If (Me.compareValues(oldValue, aVAlue) <> 0 And aVAlue <> Nothing) Or FORCE Then
                    If MAPPING.ContainsKey(Me.ordinal.Value) Then
                        aVAlue = MAPPING.Item(Me.ordinal.Value)
                        MAPPING.Remove(key:=Me.ordinal.Value)
                    End If
                    ' variable is now the value of the map
                    VARIABLE = oldValue
                    Call Me.convertValue2DB(inValue:=aVAlue, outvalue:=VARIABLE, existingValue:=True, MSGLOG:=MSGLOG)
                    Call MAPPING.Add(key:=Me.ordinal.Value, value:=aVAlue)
                    runXChange = True
                    Exit Function
                Else
                    runXChange = False
                End If

                ' set it to Nothing
            ElseIf Me.XChangeCmd = otXChangeCommandType.Delete Then
                aVAlue = Nothing
                Call Me.convertValue2DB(aVAlue, VARIABLE, MSGLOG:=MSGLOG)
                If MAPPING.ContainsKey(Me.ordinal.Value) Then
                    MAPPING.Remove(Me.ordinal.Value)
                End If
                Call MAPPING.Add(key:=Me.ordinal.Value, value:=aVAlue)
                ' exchange the variable with the value in the map
            ElseIf Me.XChangeCmd = otXChangeCommandType.Read Then
                ' donot use the variable if there is value in the oldvalue
                If Not IsEmpty(oldValue) And oldValue <> Nothing Then VARIABLE = oldValue

                Call Me.convertValue4DB(VARIABLE, aVAlue, MSGLOG)
                If MAPPING.ContainsKey(Me.ordinal.Value) Then
                    MAPPING.Remove(Me.ordinal.Value)
                End If
                Call MAPPING.Add(key:=Me.ordinal.Value, value:=aVAlue)
                VARIABLE = aVAlue
            End If

        End Function

    End Class

    '************************************************************************************
    '***** CLASS clsotdbXoutline is the object for a OTDBRecord (which is the datastore)
    '*****
    '*****
    ''' <summary>
    ''' describes a XChange Outline data structure
    ''' </summary>
    ''' <remarks></remarks>
    Public Class XOutline
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable
        Implements IEnumerable(Of XOutlineItem)

        <ormSchemaTableAttribute(Version:=1)> Public Const constTableID = "tblXOutlines"
        <ormSchemaColumnAttribute(ID:="otl1", primaryKeyordinal:=1,
                typeid:=otFieldDataType.Text, size:=50,
                description:="identifier of the outline", Title:="ID")> Public Const constFNID = "id"
        <ormSchemaColumnAttribute(ID:="otl2",
               typeid:=otFieldDataType.Text, size:=255,
                description:="description of the outline", Title:="description")> Public Const constFNdesc = "desc"
        <ormSchemaColumnAttribute(ID:="otl3",
                       typeid:=otFieldDataType.Bool,
                        description:="True if deliverable revisions are added dynamically", Title:="DynRev")> Public Const constFNRev = "addrev"


        ' key
        <ormColumnMappingAttribute(Fieldname:=constFNID)> Private _id As String = ""
        <ormColumnMappingAttribute(Fieldname:=constFNdesc)> Private _desc As String = ""
        <ormColumnMappingAttribute(Fieldname:=constFNRev)> Private _DynamicAddRevisions As Boolean
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
            If Not _IsLoaded And Not Me.IsCreated Then
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
        Public Overrides Function Initialize() As Boolean
            _IsInitialized = MyBase.Initialize
            s_cmids = New OrderedDictionary()
            Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
            Return _IsInitialized
        End Function
        ''' <summary>
        ''' deletes the object and components from the datastore
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Delete() As Boolean
            Dim initialEntry As New XOutlineItem
            Dim m As Object

            If Not Me.IsCreated And Not _IsLoaded Then
                Delete = False
                Exit Function
            End If

            ' delete each entry
            For Each anEntry As XOutlineItem In s_cmids.Values
                anEntry.Delete()
            Next
            '* delete itself
            MyBase.Delete()

            ' reset it
            s_cmids.Clear()

            _IsCreated = True
            Me.IsDeleted = True
            Me.Unload()

        End Function

        ''' <summary>
        ''' ordinals of the components
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ordinals() As Collection

            If Not Me.IsCreated And Not _IsLoaded Then
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

            If Not Me.IsCreated And Not _IsLoaded Then
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
            Return ormDataObject.CreateSchema(Of XOutline)()
        End Function
        ''' <summary>
        ''' loads the X Outline from the datastore
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal id As String) As Boolean
            Dim pkarry() As Object = {id}

            If MyBase.LoadBy(pkArray:=pkarry) Then
                _IsLoaded = _IsLoaded And LoadItems(id:=id)
            End If

            Return _IsLoaded
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
                        Call CoreMessageHandler(message:="a clsOTDBXOutlineItem couldnot be added to an outline", arg1:=anEntry.ToString,
                                                 entryname:=id, tablename:=constTableID, messagetype:=otCoreMessageType.InternalError,
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
        Public Overloads Function Persist(Optional ByVal timestamp As Date = ConstNullDate) As Boolean
            Try
                Persist = MyBase.Persist(timestamp:=timestamp)
                If Persist Then
                    ' save each entry
                    For Each anEntry As XOutlineItem In s_cmids.Values
                        'Dim anEntry As clsOTDBXOutlineItem = kvp.Value
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
            If Not _IsLoaded And Not Me.IsCreated Then
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
                    If LCase(key.ID) = "uid" Or LCase(key.ID) = "sc2" Then
                        aFirstRevision = New Deliverable
                        If aFirstRevision.LoadBy(uid:=CLng(key.Value)) Then
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

            If Not _IsLoaded And Not Me.IsCreated Then
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
                        If LCase(key.ID) = "uid" Or LCase(key.ID) = "sc2" Then
                            aFirstRevision = New Deliverable
                            If Me.DynamicAddRevisions AndAlso aFirstRevision.LoadBy(uid:=CLng(key.Value)) Then
                                If aFirstRevision.IsFirstRevision And Not aFirstRevision.IsDeleted Then
                                    ' add all revisions inclusive the follow ups
                                    For Each uid As Long In Deliverable.AllRevisionUIDsBy(aFirstRevision.Uid)
                                        Dim newKey As New XOutlineItem.OTLineKey(otFieldDataType.[Long], "uid", uid)
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

    Public Class XOutlineItem
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        ''' <summary>
        ''' Gets or sets the text.
        ''' </summary>
        ''' <value>The text.</value>
        Public Property Text() As String
            Get
                Return Me._text
            End Get
            Set(value As String)
                Me._text = Value
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
                Me._isText = Value
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
                Me._isGroup = Value
            End Set
        End Property

        ''' <summary>
        ''' OutlineKey Class as subclass of outline item to make it flexible
        ''' </summary>
        ''' <remarks></remarks>
        Public Class OTLineKey
            Private _Value As Object
            Private _ID As String
            Private [_Type] As otFieldDataType

            Public Sub New(ByVal [Type] As otFieldDataType, ByVal ID As String, ByVal value As Object)
                _Value = value
                _ID = ID
                _Type = [Type]
            End Sub
            ''' <summary>
            ''' Gets the type.
            ''' </summary>
            ''' <value>The type.</value>
            Public ReadOnly Property Type() As otFieldDataType
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

        <ormSchemaTableAttribute(version:=1)> Public Const constTableID = "tblXOutlineItems"
        <ormSchemaIndexAttribute(columnname1:=constFNID, columnname2:=ConstFNordinall)> Public Const constIndexLongOutline = "longOutline"
        <ormSchemaIndexAttribute(columnname1:=ConstFNUid, columnname2:="id", columnname3:=ConstFNordinals)> Public Const constIndexUsedOutline = "UsedOutline"

        <ormSchemaColumnAttribute(iD:="otl1", primaryKeyordinal:=1, typeid:=otFieldDataType.Text, size:=50,
            title:="Outline ID", description:="identifier of the outline")> Public Const constFNID = "id"

        <ormSchemaColumnAttribute(iD:="otli2", typeid:=otFieldDataType.Long,
           title:="ordinal", description:="ordinal as long of the outline")> Public Const ConstFNordinall = "ordiall"

        <ormSchemaColumnAttribute(iD:="otli3", primaryKeyordinal:=2, typeid:=otFieldDataType.Text, size:=255,
          title:="ordinals", description:="ordinal as string of the outline item")> Public Const ConstFNordinals = "ordials"

        <ormSchemaColumnAttribute(iD:="dlvuid", typeid:=otFieldDataType.Long,
         title:="deliverable uid", description:="uid of the deliverable")> Public Const ConstFNUid = "uid"

        <ormSchemaColumnAttribute(iD:="otli4", typeid:=otFieldDataType.Long,
          title:="identlevel", description:="identlevel as string of the outline")> Public Const ConstFNIdent = "level"

        <ormSchemaColumnAttribute(iD:="otli10", typeid:=otFieldDataType.Text, size:=255, IsArray:=True,
         title:="Types", description:="types the outline key")> Public Const ConstFNTypes = "types"

        <ormSchemaColumnAttribute(iD:="otli11", typeid:=otFieldDataType.Text, size:=255, IsArray:=True,
         title:="IDs", description:="ids the outline key")> Public Const ConstFNIDs = "ids"


        <ormSchemaColumnAttribute(iD:="otli12", typeid:=otFieldDataType.Text, size:=255, IsArray:=True,
        title:="Values", description:="values the outline key")> Public Const ConstFNValues = "values"

        <ormSchemaColumnAttribute(iD:="otli13", typeid:=otFieldDataType.Bool,
        title:="Grouping Item", description:="check if this an grouping item")> Public Const ConstFNisgroup = "isgrouped"

        <ormSchemaColumnAttribute(iD:="otli14", typeid:=otFieldDataType.Bool,
       title:="Text Item", description:="check if this an text item")> Public Const ConstFNisText = "istext"

        <ormSchemaColumnAttribute(iD:="otli14", typeid:=otFieldDataType.Text, size:=255,
       title:="Text", description:="Text if a text item")> Public Const ConstFNText = "text"

        <ormColumnMappingAttribute(fieldname:=constFNID)> Private _id As String = ""   ' ID of the outline

        Private _keys As New List(Of OTLineKey)    'keys and values
        Private _ordinal As Ordinal ' extramapping

        <ormColumnMappingAttribute(fieldname:=ConstFNIdent)> Private _level As Long = 0
        <ormColumnMappingAttribute(fieldname:=ConstFNisgroup)> Private _isGroup As Boolean
        <ormColumnMappingAttribute(fieldname:=ConstFNisText)> Private _isText As Boolean
        <ormColumnMappingAttribute(fieldname:=ConstFNText)> Private _text As String = ""

#Region "properties"

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
        ''' Initialize the DataObject
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean
            _IsInitialized = MyBase.Initialize
            Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
            Return _IsInitialized
        End Function
        ''' <summary>
        ''' infuses the data object by record
        ''' </summary>
        ''' <param name="record"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean

            Dim aType As otFieldDataType
            Dim aValue As Object


            '***
            Try
                If MyBase.Infuse(record) Then

                    aValue = record.GetValue(ConstFNordinals)

                    If IsNumeric(aValue) Then
                        _ordinal = New Ordinal(CLng(record.GetValue(ConstFNordinall)))
                    Else
                        _ordinal = New Ordinal(CStr(record.GetValue(ConstFNordinall)))
                    End If

                    ' get the keys and values
                    Dim idstr As String = record.GetValue(ConstFNIDs)
                    Dim ids As String()
                    If idstr <> "" AndAlso Not IsNull(idstr) Then
                        ids = SplitMultbyChar(idstr, ConstDelimiter)
                    Else
                        ids = {}
                    End If
                    Dim valuestr As String = record.GetValue(ConstFNValues)
                    Dim values As String()
                    If valuestr <> "" AndAlso Not IsNull(valuestr) Then
                        values = SplitMultbyChar(valuestr, ConstDelimiter)
                    Else
                        values = {}
                    End If
                    Dim typestr As String = record.GetValue(ConstFNTypes)
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
                                    Case CLng(otFieldDataType.Bool)
                                        aType = otFieldDataType.Bool
                                        aValue = CBool(values(i))
                                    Case CLng(otFieldDataType.[Date]), CLng(otFieldDataType.[Timestamp]), CLng(otFieldDataType.Time)
                                        aType = otFieldDataType.[Date]
                                        aValue = CDate(values(i))
                                    Case CLng(otFieldDataType.Text)
                                        aType = otFieldDataType.Text
                                        aValue = values(i)
                                    Case CLng(otFieldDataType.[Long])
                                        aType = otFieldDataType.[Long]
                                        aValue = CLng(values(i))
                                    Case Else
                                        Call CoreMessageHandler(subname:="clsotdbXoutlineItem.infuse", messagetype:=otCoreMessageType.InternalError,
                                                                message:="Outline datatypes couldnot be determined ", arg1:=types(i))
                                End Select

                            Catch ex As Exception
                                Call CoreMessageHandler(exception:=ex, subname:="clsotdbXoutlineItem.infuse",
                                                        messagetype:=otCoreMessageType.InternalError, message:="Outline keys couldnot be filled ")
                            End Try

                            '**
                            _keys.Add(New OTLineKey(aType, ids(i), aValue))
                        End If
                    Next


                End If

                Return Me.IsLoaded
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="clsOTDBXOutlineItem.Infuse")
                Unload()
                Return Me.IsLoaded
            End Try


            Return False

        End Function
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
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@ID", fieldname:=constFNID, tablename:=constTableID))
                    aCommand.Prepare()
                End If

                aCommand.SetParameterValue(ID:="@ID", value:=id)
                aRecordCollection = aCommand.RunSelect

                If aRecordCollection.Count > 0 Then
                    ' records read
                    For Each aRecord In aRecordCollection
                        ' add the Entry as Component
                        anEntry = New XOutlineItem
                        If anEntry.Infuse(aRecord) Then
                            aCollection.Add(value:=anEntry, key:=anEntry.ordinal)
                        End If
                    Next aRecord

                End If
                Return aCollection
            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsOTDBXoutlineItem.allByID", arg1:=id,
                                        exception:=ex, tablename:=constTableID)
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
        Public Overloads Function LoadBy(ByVal id As String, ByVal ordinal As String) As Boolean
            Return LoadBy(id, New Ordinal(ordinal))
        End Function
        Public Overloads Function LoadBy(ByVal id As String, ByVal ordinal As Long) As Boolean
            Return LoadBy(id, New Ordinal(ordinal))
        End Function
        Public Overloads Function LoadBy(ByVal id As String, ByVal ordinal As Ordinal) As Boolean
            Dim pkarry() As Object = {id, ordinal.ToString}
            Return MyBase.LoadBy(pkarry)
        End Function
        ''' <summary>
        ''' create schema for persistency
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean

            Return ormDataObject.CreateSchema(Of XOutlineItem)()


            ''''''''''''''''''''''''''''
            ''' THIS IS ONLY FOR LEGACY
            ''' 
            Dim UsedColumnNames As New Collection
            Dim aFieldDesc As New ormFieldDescription
            Dim PrimaryColumnNames As New Collection
            Dim LongOutlineColumnNames As New Collection
            Dim aTable As New ObjectDefinition
            Dim aTableEntry As New ObjectEntryDefinition


            aFieldDesc.ID = ""
            aFieldDesc.Parameter = ""
            aFieldDesc.Relation = New String() {}
            aFieldDesc.Aliases = New String() {}
            aFieldDesc.Tablename = constTableID


            aTable = New ObjectDefinition
            aTable.Create(constTableID)

            '******
            '****** Fields

            With aTable


                On Error GoTo error_handle


                '*** TaskUID
                '****
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "outline id"
                aFieldDesc.ID = ""
                aFieldDesc.Parameter = ""
                aFieldDesc.ColumnName = "id"
                aFieldDesc.ID = "otl1"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
                LongOutlineColumnNames.Add(aFieldDesc.ColumnName)

                'Position
                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "ordinal long"
                aFieldDesc.ColumnName = "ordinall"
                aFieldDesc.ID = "otli2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                LongOutlineColumnNames.Add(aFieldDesc.ColumnName)

                'Position
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "ordinal string"
                aFieldDesc.ColumnName = "ordinals"
                aFieldDesc.ID = "otli3"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                'uid
                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "deliverable uid"
                aFieldDesc.ColumnName = "uid"
                aFieldDesc.ID = "dlvuid"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                UsedColumnNames.Add(aFieldDesc.ColumnName)
                UsedColumnNames.Add("id")
                UsedColumnNames.Add("ordinals")

                ' level
                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "identlevel"
                aFieldDesc.ColumnName = "level"
                aFieldDesc.ID = "otli4"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' typeid
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "types of the outline key"
                aFieldDesc.ColumnName = "types"
                aFieldDesc.ID = "otli10"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' id
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "ids of the outline key"
                aFieldDesc.ColumnName = "ids"
                aFieldDesc.ID = "otli11"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' value #1
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "values of the outline key"
                aFieldDesc.ColumnName = "values"
                aFieldDesc.ID = "otli12"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.ID = ""
                aFieldDesc.Parameter = ""
                aFieldDesc.Relation = New String() {}
                aFieldDesc.Aliases = New String() {}

                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.ColumnName = ConstFNCreatedOn
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' Index
                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
                Call .AddIndex("longOutline", LongOutlineColumnNames, isprimarykey:=False)
                Call .AddIndex("UsedOutline", UsedColumnNames, isprimarykey:=False)
                ' persist
                .Persist()
                ' change the database
                .AlterSchema()
            End With

            CreateSchema = True
            Exit Function

            ' Handle the error
error_handle:
            Call CoreMessageHandler(subname:="clsOTDBXOutlineItem.createSchema", tablename:=constTableID)
            CreateSchema = False
        End Function
        ''' <summary>
        ''' Persist the data object to the datastore
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Overloads Function Persist(Optional timestamp As Date = ConstNullDate) As Boolean

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
                If _ordinal.Type = ordinalType.longType Then
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
                    If LCase(key.ID) = "uid" Then
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
                CoreMessageHandler(exception:=ex, subname:="clsOTDBXoutlineItem.persist")
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
                    _keys.Add(New OTLineKey(otFieldDataType.Long, "uid", uid))
                End If

                _level = level
                Return Me.IsCreated
            End If

            Return False
        End Function
    End Class
End Namespace
