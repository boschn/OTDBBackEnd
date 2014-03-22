
REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** BUSINESS OBJECTs: CONFIGURABLES Classes for On Track Database Backend Library
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2013-08-08
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** TO DO Log:
REM ***********             - unfinished
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************

Option Explicit On
Imports System.Diagnostics
Imports System.Linq
Imports System.Diagnostics.Debug

Imports OnTrack
Imports OnTrack.Database
Imports OnTrack.XChange
Imports OnTrack.Scheduling
Imports OnTrack.Deliverables

Namespace OnTrack.Configurables


    '************************************************************************************
    '***** CLASS clsOTDBDefConfiguration is the object for a OTDBRecord (which is the datastore)
    '*****
    '*****

    Class clsOTDBDefConfiguration
        Inherits ormDataObject

        Const ourTableName = "tblDefConfigItems"

        ' key
        Private s_configname As String
        ' components itself per key:=posno, item:=cmid
        Private s_items As New Dictionary(Of String, clsOTDBDefConfigurationItem)
        Private s_aliases As New Dictionary(Of String, String)

        '** initialize
        Public Sub New()
            MyBase.New(ourTableName)
        End Sub



        ReadOnly Property CONFIGNAME()
            Get
                CONFIGNAME = s_configname
            End Get

        End Property

        ReadOnly Property NoItems() As Long
            Get
                NoItems = s_items.Count - 1
            End Get

        End Property

        '*** init
        Public Function initialize() As Boolean
            initialize = MyBase.Initialize

        End Function
        '***** getMileStoneIDByAlias returns the ID on a given AliasID
        '***** blank otherwise
        Public Function getIDByAlias(AliasID As String) As String

            If s_aliases.ContainsKey(key:=LCase(AliasID)) Then
                getIDByAlias = s_aliases.Item(key:=LCase(AliasID))
                Exit Function
            End If

            getIDByAlias = ""
        End Function

        '*** addItemByValues
        '***
        Public Function addItemByValues(ByVal ID As String, _
                                        ByVal DATATYPE As otFieldDataType, _
                                        Optional ByVal TYPEID As String = "", _
                                        Optional ByVal PARAMETER As String = "", _
                                        Optional ByVal aliases As String() = Nothing, _
                                        Optional ByVal TITLE As String = "", _
                                        Optional ByVal COMMENT As String = "") As Boolean

            If Not IsCreated And Not IsLoaded Then
                addItemByValues = False
                Exit Function
            End If

            Dim anItem As New clsOTDBDefConfigurationItem

            If Not anItem.create(CONFIGNAME:=Me.CONFIGNAME, ID:=ID) Then
                Call anItem.Inject(CONFIGNAME:=Me.CONFIGNAME, ID:=ID)
            End If

            With anItem
                .DATATYPE = DATATYPE
                .PARAMETER = PARAMETER
                .aliases = aliases
                .COMMENT = COMMENT
                '.relation = relation
                '.size = size
                .TITLE = TITLE
            End With
            addItemByValues = Me.addItem(ITEM:=anItem)
        End Function


        '*** addItem
        '***
        Public Function addItem(ByRef ITEM As clsOTDBDefConfigurationItem) As Boolean
            Dim flag As Boolean
            Dim existEntry As New clsOTDBDefConfigurationItem
            Dim aTableEntry As AbstractEntryDefinition
            Dim aKeyCollection As New Collection

            Dim m As Object

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                addItem = False
                Exit Function
            End If

            ' remove and overwrite
            If s_items.ContainsKey(key:=ITEM.ID) Then
                Call s_items.Remove(key:=ITEM.ID)
            End If
            ' load aliases
            If aTableEntry.Inject(objectname:=ourTableName, entryname:=ITEM.ID) Then
                For Each m In aTableEntry.Aliases
                    If s_aliases.ContainsKey(key:=LCase(m)) Then
                        Call s_aliases.Remove(key:=LCase(m))
                    End If
                    Call s_aliases.Add(key:=LCase(m), value:=ITEM.ID)
                Next m
            End If
            ' add entry
            s_items.Add(key:=ITEM.ID, value:=ITEM)

            '
            addItem = True

        End Function

        '**** delete
        '****
        Public Function delete() As Boolean
            Dim anEntry As New clsOTDBDefConfigurationItem
            Dim initialEntry As New clsOTDBDefConfigurationItem
            Dim m As Object

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    delete = False
                    Exit Function
                End If
            End If
            If Me.IsCreated Then
                Call Me.Inject(CONFIGNAME:=Me.CONFIGNAME)
            End If
            If Not _IsLoaded And Not Me.IsCreated Then
                delete = False
                Exit Function
            End If

            ' delete each entry
            For Each kvp As KeyValuePair(Of String, clsOTDBDefConfigurationItem) In s_items
                anEntry = kvp.Value
                anEntry.Delete()
            Next

            ' reset it
            s_items = New Dictionary(Of String, clsOTDBDefConfigurationItem)
            If Not anEntry.create(CONFIGNAME:=s_configname, ID:="") Then
                Call anEntry.Inject(CONFIGNAME:=s_configname, ID:="")
                anEntry.ID = ""

            End If
            s_items.Add(key:="", value:=anEntry)

            _IsCreated = True
            Me.IsDeleted = True
            Me.Unload()

        End Function

        '**** IDs
        '****
        Public Function IDs() As String()

            If Not Me.IsCreated And Not _IsLoaded Then
                IDs = Nothing
                Exit Function
            End If

            ' delete each entry
            IDs = s_items.Keys.ToArray


        End Function

        '**** Item(ID) returns that item or nothing
        '****
        Public Function ITEM(ByVal ID As String) As clsOTDBDefConfigurationItem
            Dim anEntry As New clsOTDBDefConfigurationItem


            If Not Me.IsCreated And Not _IsLoaded Then
                ITEM = Nothing
                Exit Function
            End If

            ' delete each entry
            If s_items.ContainsKey(key:=ID) Then
                ITEM = s_items.Item(key:=ID)
                Exit Function
            Else
                ITEM = Nothing
                Exit Function
            End If

        End Function
        '**** Items returns a Collection of Items
        '****
        Public Function Items() As Collection
            Dim anEntry As New clsOTDBDefConfigurationItem
            Dim aCollection As New Collection
            Dim m As Object

            If Not Me.IsCreated And Not _IsLoaded Then
                Items = Nothing
                Exit Function
            End If

            ' delete each entry

            For Each kvp As KeyValuePair(Of String, clsOTDBDefConfigurationItem) In s_items
                anEntry = kvp.Value
                If anEntry.ID <> "" Then
                    aCollection.Add(anEntry)
                End If
            Next

            Items = aCollection
        End Function

        '**** infuese the object by a OTDBRecord
        '****
        Public Function infuse(ByRef aRecord As ormRecord) As Boolean
            ' not implemented
            infuse = False
        End Function



        '**** Inject : load the object by the PrimaryKeys
        '****
        Public Function Inject(ByVal CONFIGNAME As String) As Boolean
            Dim aTable As iormDataStore
            Dim aRecordCollection As List(Of ormRecord)
            Dim aRecord As ormRecord
            Dim aIndexCollection As New Collection

            Dim anEntry As New clsOTDBDefConfigurationItem

            Dim wherestr As String
            'Dim PKArry(1 To 1) As Object

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    Inject = False
                    Exit Function
                End If
            End If

            ' set the primaryKey

            aTable = GetTableStore(ourTableName)
            aRecordCollection = aTable.GetRecordsBySql(wherestr:="cname = '" & CONFIGNAME & "'")
            'Set aRecordCollection = aTable.getRecordsByIndex(aTable.primaryKeyIndexName, Key, True)

            If aRecordCollection Is Nothing Then
                Me.Unload()
                Inject = False
                Exit Function
            Else
                s_configname = CONFIGNAME
                _IsLoaded = True
                ' records read
                For Each aRecord In aRecordCollection
                    ' add the Entry as Component
                    anEntry = New clsOTDBDefConfigurationItem
                    If anEntry.infuse(aRecord) Then
                        If Not Me.addItem(anEntry) Then
                        End If
                    End If
                Next aRecord
                '
                _IsLoaded = True
                Inject = True
                Exit Function
            End If

error_handler:
            Me.Unload()
            Inject = True
            Exit Function
        End Function

        '**** persist
        '****

        Public Function Persist(Optional ByVal timestamp As Date = ot.ConstNullDate) As Boolean
            Dim anEntry As New clsOTDBDefConfigurationItem
            Dim aTimestamp As Date
            Dim headentry As New clsOTDBDefConfigurationItem
            Dim anIndexColl As Collection
            Dim i As Integer
            Dim flag As Boolean
            Dim changed As Boolean
            Dim m As Object
            Dim n As Object

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    Persist = False
                    Exit Function
                End If
            End If
            If Not _IsLoaded And Not Me.IsCreated Then
                Persist = False
                Exit Function
            End If

            If IsMissing(timestamp) Then
                'set Timestamp
                aTimestamp = Now
            Else
                aTimestamp = timestamp
            End If
            '
            ' persist each entry
            For Each kvp As KeyValuePair(Of String, clsOTDBDefConfigurationItem) In s_items
                anEntry = kvp.Value
                If anEntry.ID <> "" Then
                    If anEntry.IsChanged Then
                        changed = True
                    End If
                    ' persist member first
                    anEntry.Persist(aTimestamp)
                Else
                    headentry = anEntry
                End If
            Next
            ' persist head
            If changed Then headentry.incversion()
            Persist = headentry.Persist(aTimestamp)

            Exit Function

errorhandle:

            Persist = False

        End Function

        '**** create : create a new Object with primary keys
        '****
        Public Function create(ByVal CONFIGNAME As String) As Boolean
            Dim anEntry As New clsOTDBDefConfigurationItem

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    create = False
                    Exit Function
                End If
            End If

            If IsLoaded Then
                create = False
                Exit Function
            End If

            ' set the primaryKey
            s_configname = CONFIGNAME
            s_items = New Dictionary(Of String, clsOTDBDefConfigurationItem)
            ' abort create if exists
            If Not anEntry.create(CONFIGNAME:=CONFIGNAME, ID:="") Then
                create = False
                Exit Function
            End If
            s_items.Add(key:="", value:=anEntry)

            _IsCreated = True
            create = Me.IsCreated

        End Function

        '**** Singleton -> returns aCached <CONFIGNAME> Schedule Definition
        '****
        'Public Function Singleton(ByVal CONFIGNAME As String) As clsOTDBDefConfiguration
        '    Dim aCachedObject As New clsOTDBDefConfiguration
        '    Dim aVAlue As Object

        '    aVAlue = loadFromCache(ourTableName, New String() {"SINGLETON", CONFIGNAME})
        '    If aVAlue Is Nothing Then
        '        If Not aCachedObject.Inject(CONFIGNAME:=CONFIGNAME) Then
        '            aCachedObject = Nothing
        '        Else
        '            Call AddToCache(ourTableName, New String() {"SINGLETON", CONFIGNAME}, theOBJECT:=aCachedObject)

        '        End If
        '    Else
        '        aCachedObject = aVAlue
        '    End If

        '    Singleton = aCachedObject
        '    Exit Function
        'End Function


    End Class

    Class clsOTDBDefConfigurationItem

        Inherits ormDataObject

        '************************************************************************************
        '***** CLASS clsOTDBDefConfigurationItem describes additional database schema information
        '*****

        Const ourTableName = "tblDefConfigItems"

        ' fields
        Private s_configname As String
        Private s_id As String
        Private s_relation As String
        Private s_parameter As String
        Private s_aliases As String
        Private s_title As String
        Private s_datatype As otFieldDataType
        Private s_version As Long
        Private s_size As Long
        Private s_typeid As String
        Private s_cmt As String


        ' further internals


        '** initialize
        Public Sub New()
            MyBase.New(ourTableName)


        End Sub


        ReadOnly Property CONFIGNAME() As String
            Get
                CONFIGNAME = s_configname
            End Get
        End Property
        'Public Property Let configname(aValue As String)
        '    If LCase(s_configname) <> LCase(aValue) Then
        '        s_configname = aValue
        '        me.ischanged = True
        '    End If
        'End Property
        Public Property ID() As String
            Get
                ID = s_id
            End Get
            Set(value As String)
                If LCase(s_id) <> LCase(value) Then
                    s_id = LCase(value)
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property TYPEID() As String
            Get
                TYPEID = s_typeid
            End Get
            Set(value As String)
                If s_typeid <> value Then
                    s_typeid = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property DATATYPE() As otFieldDataType
            Get
                DATATYPE = s_datatype
            End Get
            Set(value As otFieldDataType)
                If s_datatype <> value Then
                    s_datatype = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property version() As Long
            Get
                version = s_version
            End Get
            Set(value As Long)
                If s_version <> value Then
                    s_version = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property size() As Long
            Get
                size = s_size
            End Get
            Set(value As Long)
                If s_size <> value Then
                    s_size = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property aliases() As String()
            Get
                aliases = SplitMultbyChar(text:=s_aliases, DelimChar:=ConstDelimiter)
                If Not IsArrayInitialized(aliases) Then
                    aliases = New String() {}
                End If
            End Get
            Set(avalue As String())
                Dim i As Integer
                If IsArrayInitialized(avalue) Then
                    Dim aStrValue As String
                    For i = LBound(avalue) To UBound(avalue)
                        If i = LBound(avalue) Then
                            aStrValue = ConstDelimiter & UCase(avalue(i)) & ConstDelimiter
                        Else
                            aStrValue = aStrValue & UCase(avalue(i)) & ConstDelimiter
                        End If
                    Next i
                    s_aliases = aStrValue
                    IsChanged = True
                    'ElseIf Not IsNothing(Trim(avalue)) And Trim(avalue) <> "" And Not isNull(avalue) Then
                    's_aliases = ConstDelimiter & UCase(Trim(avalue)) & ConstDelimiter
                Else
                    s_aliases = ""
                End If
            End Set
        End Property


        Public Property PARAMETER() As String
            Get
                PARAMETER = s_parameter
            End Get
            Set(value As String)
                If LCase(s_parameter) <> LCase(value) Then
                    s_parameter = value
                    Me.IsChanged = True
                End If
            End Set
        End Property


        Public Property COMMENT() As String
            Get
                COMMENT = s_cmt
            End Get
            Set(value As String)
                If LCase(s_cmt) <> LCase(value) Then
                    s_cmt = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property relation() As Object()
            Get
                relation = SplitMultbyChar(text:=s_relation, DelimChar:=ConstDelimiter)
                If Not IsArrayInitialized(relation) Then
                    relation = New String() {}
                End If
            End Get
            Set(avalue As Object())
                Dim i As Integer
                If IsArrayInitialized(avalue) Then
                    Dim aStrValue As String
                    For i = LBound(avalue) To UBound(avalue)
                        If i = LBound(avalue) Then
                            aStrValue = ConstDelimiter & UCase(avalue(i)) & ConstDelimiter
                        Else
                            aStrValue = aStrValue & avalue(i) & ConstDelimiter
                        End If
                    Next i
                    s_relation = aStrValue
                    IsChanged = True
                    'ElseIf Not IsNothing(Trim(aVAlue)) And Trim(aVAlue) <> "" And Not isNull(aVAlue) Then
                    '   s_relation = ConstDelimiter & CStr(Trim(avalue)) & ConstDelimiter
                Else
                    s_relation = ""
                End If
            End Set
        End Property

        Public Property TITLE() As String
            Get
                TITLE = s_title
            End Get
            Set(value As String)
                If s_title <> value Then
                    s_title = value
                    Me.IsChanged = True
                End If
            End Set
        End Property


        '*** init
        Public Function initialize() As Boolean
            initialize = MyBase.Initialize

        End Function

        '*** inc version
        Public Function incversion() As Long
            s_version = s_version + 1
            incversion = s_version
        End Function

        '**** infuese the object by a OTDBRecord
        '****
        Public Function infuse(ByRef aRecord As ormRecord) As Boolean
            Dim aVAlue As String

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    infuse = False
                    Exit Function
                End If
            End If


            On Error GoTo errorhandle

            Me.Record = aRecord
            s_configname = CStr(aRecord.GetValue("cname"))
            s_id = CStr(aRecord.GetValue("id"))
            s_parameter = CStr(aRecord.GetValue("parameter"))
            s_relation = CStr(aRecord.GetValue("relation"))
            s_typeid = CStr(aRecord.GetValue("typeid"))
            s_datatype = CLng(aRecord.GetValue("datatype"))
            s_version = CLng(aRecord.GetValue("updc"))
            s_title = CStr(aRecord.GetValue("title"))
            s_aliases = CStr(aRecord.GetValue("alias"))
            s_cmt = CStr(aRecord.GetValue("cmt"))


            infuse = MyBase.Infuse(aRecord)
            _IsLoaded = infuse
            Exit Function

errorhandle:
            infuse = False


        End Function

        '**** allByID
        '****
        Public Function allByID(ByVal ID As String, Optional ByVal CONFIGNAME As String = "") As Collection
            Dim aCollection As New Collection
            Dim aRecordCollection As List(Of ormRecord)
            Dim returnCollection As New Collection
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim wherestr As String = ""
            Dim aNew As New clsOTDBDefConfigurationItem

            '* lazy init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    allByID = Nothing
                    Exit Function
                End If
            End If

            On Error GoTo error_handler

            aTable = GetTableStore(Me.TableID)
            wherestr = " ( ID = '" & UCase(ID) & "' or alias like '%" & ConstDelimiter & UCase(ID) & ConstDelimiter & "%' )"
            If CONFIGNAME <> "" Then
                wherestr = wherestr & " and cname = '" & CONFIGNAME & "'"
            End If
            aRecordCollection = aTable.GetRecordsBySql(wherestr:=wherestr)

            If aRecordCollection Is Nothing Then
                Me.Unload()
                allByID = Nothing
                Exit Function
            Else
                For Each aRecord In aRecordCollection

                    aNew = New clsOTDBDefConfigurationItem
                    If aNew.infuse(aRecord) Then
                        aCollection.Add(Item:=aNew)
                    End If
                Next aRecord
                allByID = aCollection
                Exit Function
            End If

error_handler:

            allByID = Nothing
            Exit Function
        End Function
        '**** loadByID
        '****
        Public Function loadByID(ByVal ID As String, Optional ByVal CONFIGNAME As String = "") As Boolean
            Dim aCollection As New Collection
            Dim aRecordCollection As List(Of ormRecord)
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim wherestr As String

            '* lazy init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    loadByID = False
                    Exit Function
                End If
            End If

            On Error GoTo error_handler

            aTable = GetTableStore(Me.TableID)
            wherestr = " ( ID = '" & UCase(ID) & "' or alias like '%" _
                       & ConstDelimiter & UCase(ID) & ConstDelimiter & "%' )"
            If CONFIGNAME <> "" Then
                wherestr = wherestr & " and cname = '" & CONFIGNAME & "'"
            End If
            aRecordCollection = aTable.GetRecordsBySql(wherestr:=wherestr)

            If aRecordCollection Is Nothing Then
                Me.Unload()
                loadByID = False
                Exit Function
            Else
                For Each aRecord In aRecordCollection
                    ' take the first
                    If infuse(aRecord) Then
                        loadByID = True
                        Exit Function
                    End If
                Next aRecord
                loadByID = False
                Exit Function
            End If

error_handler:

            loadByID = False
            Exit Function
        End Function
        '**** Inject : load the object by the PrimaryKeys
        '****
        Public Function Inject(ByVal CONFIGNAME As String, ByVal ID As String) As Boolean
            Dim aTable As iormDataStore
            Dim pkarry(1) As Object
            Dim aRecord As ormRecord

            '* lazy init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    Inject = False
                    Exit Function
                End If
            End If

            ' set the primaryKey
            pkarry(0) = LCase(CONFIGNAME)
            pkarry(1) = LCase(ID)
            'PKArry(3) = id


            aTable = GetTableStore(ourTableName)
            ' try to load it from cache
            'aRecord = loadFromCache(ourTableName, pkarry)
            ' load it from database
            If aRecord Is Nothing Then
                aTable = GetTableStore(ourTableName)
                aRecord = aTable.GetRecordByPrimaryKey(pkarry)
            End If

            If aRecord Is Nothing Then
                Me.Unload()
                Inject = Me.IsLoaded
                Exit Function
            Else
                Me.Record = aRecord
                _IsLoaded = Me.infuse(Me.Record)
                'Call AddToCache(objectTag:=ourTableName, key:=pkarry, theOBJECT:=aRecord)

                Inject = Me.IsLoaded
                Exit Function
            End If


        End Function
        ''' <summary>
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean

'            Dim aFieldDesc As New ormFieldDescription
'            Dim PrimaryColumnNames As New Collection
'            Dim aTable As New ObjectDefinition
'            Dim IDColumnNames As New Collection

'            '

'            With aTable
'                .Create(ourTableName)
'                .Delete()

'                aFieldDesc.Tablename = ourTableName
'                aFieldDesc.ID = ""
'                aFieldDesc.Parameter = ""
'                aFieldDesc.Relation = New String() {}

'                '***
'                '*** Fields
'                '****

'                'Tablename
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "configname"
'                aFieldDesc.ColumnName = "cname"
'                aFieldDesc.Size = 50
'                Call .AddFieldDesc(aFieldDesc)
'                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

'                ' id
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "ID of config item"
'                aFieldDesc.ColumnName = "id"
'                aFieldDesc.ID = ""
'                Call .AddFieldDesc(aFieldDesc)
'                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
'                ' alias IDs
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "aliases id"
'                aFieldDesc.ColumnName = "alias"
'                aFieldDesc.ID = ""
'                aFieldDesc.Size = 0
'                Call .AddFieldDesc(aFieldDesc)

'                'title
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "title"
'                aFieldDesc.ColumnName = "title"
'                Call .AddFieldDesc(aFieldDesc)

'                'Type
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "typeid"
'                aFieldDesc.ColumnName = "typeid"
'                Call .AddFieldDesc(aFieldDesc)

'                'Parameter
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "Parameter"
'                aFieldDesc.ColumnName = "parameter"
'                Call .AddFieldDesc(aFieldDesc)

'                'datatype
'                aFieldDesc.Datatype = otFieldDataType.[Long]
'                aFieldDesc.Title = "data type of the id data field"
'                aFieldDesc.ColumnName = "datatype"
'                Call .AddFieldDesc(aFieldDesc)

'                'version
'                aFieldDesc.Datatype = otFieldDataType.[Long]
'                aFieldDesc.Title = "update / version counter"
'                aFieldDesc.ColumnName = "updc"
'                Call .AddFieldDesc(aFieldDesc)

'                'size
'                aFieldDesc.Datatype = otFieldDataType.[Long]
'                aFieldDesc.Title = "size of datafield"
'                aFieldDesc.ColumnName = "size"
'                Call .AddFieldDesc(aFieldDesc)

'                'Relation
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "Relation"
'                aFieldDesc.ColumnName = "relation"
'                Call .AddFieldDesc(aFieldDesc)

'                'comment
'                aFieldDesc.Datatype = otFieldDataType.Memo
'                aFieldDesc.Title = "comment and description"
'                aFieldDesc.ColumnName = "cmt"
'                Call .AddFieldDesc(aFieldDesc)


'                '***
'                '*** TIMESTAMP
'                '****
'                aFieldDesc.Datatype = otFieldDataType.Timestamp
'                aFieldDesc.Title = "last Update"
'                aFieldDesc.ColumnName = ConstFNUpdatedOn
'                aFieldDesc.ID = ""
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                aFieldDesc.Datatype = otFieldDataType.Timestamp
'                aFieldDesc.Title = "creation Date"
'                aFieldDesc.ColumnName = ConstFNCreatedOn
'                aFieldDesc.ID = ""
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                ' Index
'                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)

'                ' persist
'                .Persist()

'                ' change the database
'                .CreateObjectSchema()
'            End With


'            '
'            CreateSchema = True
'            Exit Function

'            ' Handle the error
'error_handle:
'            Call CoreMessageHandler(subname:="clsOTDBDefConfigurationItem.createSchema", tablename:=ourTableName)
'            CreateSchema = False
        End Function

        '**** persist
        '****

        Public Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean
            Dim aVAlue As Object
            Dim aConfigItem As New clsOTDBConfigurableItem
            Dim aConfig As New clsOTDBConfigurable
            Dim aCompDesc As New ormCompoundDesc

            'Dim anObjectDef As ObjectDefinition = OnTrack.ObjectDefinition.Retrieve (objectname:=aConfig.)

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    Persist = False
                    Exit Function
                End If
            End If
            If Not _IsLoaded And Not Me.IsCreated Then
                Persist = False
                Exit Function
            End If

            If Not Me.Record.Alive Then
                Persist = False
                Exit Function
            End If

            ' create compound for tblschedules
            '
            'If anObjectDef.Inject(aConfig.TableID) Then
            '    anObjectDef.Create(aConfig.TableID)
            'End If

            aCompDesc.Tablename = aConfig.TableID
            aCompDesc.compound_Tablename = aConfigItem.TableID
            aCompDesc.ID = s_id
            aCompDesc.compound_Relation = New String() {"uid"}
            aCompDesc.compound_IDFieldname = "id"
            aCompDesc.compound_ValueFieldname = "value"
            aCompDesc.Datatype = Me.DATATYPE
            aCompDesc.Aliases = Me.aliases
            aCompDesc.Parameter = Me.PARAMETER
            aCompDesc.Title = "config item of " & s_configname & "(" & s_id & ")"

            'If anObjectDef.AddEntry(aCompDesc) Then
            '    anObjectDef.Persist()
            'End If


            'On Error GoTo errorhandle
            Call Me.Record.SetValue("cname", s_configname)
            Call Me.Record.SetValue("id", s_id)
            Call Me.Record.SetValue("title", s_title)
            Call Me.Record.SetValue("size", s_size)
            Call Me.Record.SetValue("datatype", s_datatype)
            Call Me.Record.SetValue("typeid", s_typeid)

            ' increment version if changed
            If Me.IsChanged Then
                s_version = s_version + 1
            End If

            Call Me.Record.SetValue("updc", s_version)
            Call Me.Record.SetValue("relation", s_relation)
            Call Me.Record.SetValue("parameter", s_parameter)
            Call Me.Record.SetValue("alias", s_aliases)
            Call Me.Record.SetValue("cmt", s_cmt)

            ' persist with update timestamp

            Persist = Me.Record.Persist(timestamp)

            Exit Function

errorhandle:

            Persist = False

        End Function

        '**** create : create a new Object with primary keys
        '****
        Public Function create(ByVal CONFIGNAME As String, _
                               ByVal ID As String, _
                               Optional ByVal DATATYPE As otFieldDataType = 0, _
                               Optional ByVal FORCE As Boolean = False) As Boolean
            Dim aTable As iormDataStore
            Dim pkarry(2) As Object
            Dim aRecord As ormRecord

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    create = False
                    Exit Function
                End If
            End If

            If IsLoaded Then
                create = False
                Exit Function
            End If

            ' Check
            If Not FORCE Then
                ' set the primaryKey
                pkarry(0) = LCase(CONFIGNAME)
                pkarry(1) = LCase(ID)

                'PKArry(3) = dependfrompartid
                aTable = GetTableStore(ourTableName)
                aRecord = aTable.GetRecordByPrimaryKey(pkarry)

                If Not aRecord Is Nothing Then
                    create = False
                    'Call OTDBErrorHandler(tablename:=ourTableName, entryname:="partid, posno", _
                    'subname:="clsOTDBBOMMember.create", message:=" double key as should be unique", arg1:=partid & posno)
                    Exit Function
                End If
            End If


            ' set the primaryKey
            s_configname = LCase(CONFIGNAME)
            s_id = LCase(ID)
            s_datatype = DATATYPE


            _IsCreated = True
            create = Me.IsCreated


        End Function

       

    End Class


    Class clsOTDBConfigurable
        Inherits ormDataObject
        Implements iotXChangeable

        '************************************************************************************
        '***** CLASS clsOTDBConfigurable is the object for a OTDBRecord (which is the datastore)
        '*****
        '*****

        Const ourTableName = "tblConfigs"


        'Private s_serializeWithHostApplication As Boolean

        Private s_uid As Long
        Private s_configname As String
        Private s_updc As Long
        Private s_comment As String
        Private s_msglogtag As String

        ' components itself per key:=id, item:=clsOTDBXScheduleMilestone
        Private s_items As New Dictionary(Of String, clsOTDBConfigurableItem)
        Private s_orgItemValues As New Dictionary(Of String, Object)    'orgItems -> original Items before any change

        ' dynamic
        Private s_isItemChanged As Boolean

        'Private s_loadedFromHost As Boolean
        'Private s_savedToHost As Boolean
        Private s_DefConfiguration As New clsOTDBDefConfiguration

        Private s_msglog As New ObjectLog

        '** initialize
        Public Sub New()
            MyBase.New(ourTableName)

        End Sub


        ReadOnly Property UID() As Long
            Get
                UID = s_uid
            End Get

        End Property

        'Public Property Get loadedFromHost() As Boolean
        '    loadedFromHost = s_loadedFromHost
        'End Property
        Public Function getDefConfiguration() As clsOTDBDefConfiguration
            If s_DefConfiguration Is Nothing Then
                s_DefConfiguration = New clsOTDBDefConfiguration
            End If

            If Not s_DefConfiguration.IsLoaded And Not s_DefConfiguration.IsCreated Then
                's_DefConfiguration = s_DefConfiguration.Singleton(CONFIGNAME:=s_configname)
                If s_DefConfiguration Is Nothing Then
                    Call CoreMessageHandler(message:="config defintion doesn't exist", subname:="clsOTDBConfigurable.DefConfiguration", _
                                          arg1:=s_configname)
                    s_DefConfiguration = New clsOTDBDefConfiguration
                End If
            End If
            getDefConfiguration = s_DefConfiguration
        End Function

        Public Property updc() As Long
            Get
                updc = s_updc
            End Get
            Set(value As Long)
                If s_updc <> value Then
                    s_updc = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property CONFIGNAME() As String
            Get
                CONFIGNAME = s_configname
            End Get
            Set(avalue As String)
                Dim defconfiguration As clsOTDBDefConfiguration
                ' set the internal defconfiguration link
                If LCase(s_configname) <> LCase(avalue) Then
                    ' defconfiguration = s_DefConfiguration.Singleton(CONFIGNAME:=avalue)
                    If defconfiguration Is Nothing Then
                        Call CoreMessageHandler(message:="CONFIG has not been defined", subname:="clsOTDBConfigurable.CONFIGNAME", _
                                              arg1:=avalue)
                    Else
                        s_DefConfiguration = defconfiguration
                        s_configname = avalue
                        Me.IsChanged = True
                    End If
                    ' load the milestones
                    If Not loadItems(CONFIGNAME:=s_configname) Then
                        Call CoreMessageHandler(message:="Items of CONFIGNAME couldnot loaded", _
                                              subname:="clsOTDBConfigurable.CONFIGNAME let", _
                                              arg1:=avalue)
                        Exit Property
                    End If
                End If
            End Set
        End Property

        '*** increment the updc version
        Public Function incupdc() As Long
            s_updc = s_updc + 1
            incupdc = s_updc
            Me.IsChanged = True
        End Function

        Public Property COMMENT() As String
            Get
                COMMENT = s_comment
            End Get
            Set(value As String)
                s_comment = value
                Me.IsChanged = True
            End Set
        End Property

        '****** getUniqueTag
        Public Function getUniqueTag()
            getUniqueTag = ConstDelimiter & ourTableName & ConstDelimiter & s_uid & ConstDelimiter & s_updc & ConstDelimiter
        End Function
        ReadOnly Property msglogtag() As String
            Get
                If s_msglogtag = "" Then
                    s_msglogtag = getUniqueTag()
                End If
                msglogtag = s_msglogtag
            End Get
        End Property

        ReadOnly Property NoItems() As Long
            Get
                If s_items Is Nothing Then
                    Return 0

                End If

                ' No of Components
                NoItems = s_items.Count

            End Get
        End Property
        '*** init
        Public Function initialize() As Boolean
            'Call registerCacheFor(ourTableName)
            initialize = MyBase.Initialize
            s_items = New Dictionary(Of String, clsOTDBConfigurableItem)
            s_orgItemValues = New Dictionary(Of String, Object)

            s_isItemChanged = False

            'serializeWithHostApplication = isDefaultSerializeAtHostApplication(ourTableName)
            s_DefConfiguration = New clsOTDBDefConfiguration
            's_parameter_date1 = ot.ConstNullDate
            's_parameter_date2 = ot.ConstNullDate
            's_parameter_date3 = ot.ConstNullDate

        End Function

        '******* ITEM returns the ITEM ID as Object or Null if not exists
        '*******
        Public Function getItem(ByVal ID As String, Optional ORIGINAL As Boolean = False) As Object
            Dim aItem As New clsOTDBConfigurableItem
            Dim aDefConfiguration As clsOTDBDefConfiguration
            Dim aRealID As String

            If Not IsCreated And Not IsLoaded Then
                getItem = Null()
                Exit Function
            End If

            ' check aliases
            aDefConfiguration = Me.getDefConfiguration
            If aDefConfiguration Is Nothing Then
                Call CoreMessageHandler(message:="DefConfiguration is not valid", arg1:=Me.CONFIGNAME, subname:="clsOTDBConfigurable.getItem")
                getItem = Null()
                Exit Function
            End If
            aRealID = aDefConfiguration.getIDByAlias(AliasID:=LCase(ID))
            If aRealID = "" Then
                aRealID = LCase(ID)
            End If

            ' return not orgininal

            If s_items.ContainsKey(LCase(aRealID)) Then
                aItem = s_items.Item(LCase(aRealID))
                If Not ORIGINAL Then
                    getItem = aItem.Value
                ElseIf s_orgItemValues.ContainsKey(LCase(aRealID)) Then
                    getItem = s_orgItemValues.Item(LCase(aRealID))
                Else
                    getItem = Null()
                End If

            Else
                getItem = Null()
            End If


        End Function
        '******* setItem ID to Value
        '*******
        Public Function setItem(ByVal ID As String, ByVal Value As Object) As Boolean
            Dim aItem As New clsOTDBConfigurableItem
            Dim isItemchanged As Boolean
            Dim aDefConfiguration As clsOTDBDefConfiguration
            Dim aRealID As String

            If Not IsCreated And Not IsLoaded Then
                setItem = False
                Exit Function
            End If

            ' check aliases
            aDefConfiguration = Me.getDefConfiguration
            If aDefConfiguration Is Nothing Then
                Call CoreMessageHandler(message:="DefConfiguration is not valid", arg1:=Me.CONFIGNAME, subname:="clsOTDBConfigurable.getItem")
                setItem = False
                Exit Function
            End If
            aRealID = aDefConfiguration.getIDByAlias(AliasID:=LCase(ID))
            If aRealID = "" Then
                aRealID = LCase(ID)
            End If

            ' return
            If s_items.ContainsKey(LCase(aRealID)) Then
                aItem = s_items.Item(LCase(aRealID))
            Else
                Call CoreMessageHandler(arg1:=ID, subname:="clsOTDBConfigurable.setItem", tablename:=ourTableName, _
                                      message:="ID doesnot exist in Item Entries")
                setItem = False
                Exit Function
            End If

            isItemchanged = False


            ' if the Item is only a Cache ?!
            If aItem.isCacheNoSave Then
                Call CoreMessageHandler(message:="setItem to cached Item", _
                                      subname:="clsOTDBConfigurable.setItem", _
                                      arg1:=LCase(ID) & ":" & CStr(Value))
                setItem = False
                Exit Function
            End If

            ' convert it
            If (aItem.DATATYPE = otFieldDataType.[Date] Or _
                aItem.DATATYPE = otFieldDataType.Timestamp) And IsDate(Value) Then
                If aItem.Value <> CDate(Value) Then
                    aItem.Value = CDate(Value)
                    isItemchanged = True
                End If
            ElseIf aItem.DATATYPE = otFieldDataType.Numeric And IsNumeric(Value) Then
                If aItem.Value <> CDbl(Value) Then
                    aItem.Value = CDbl(Value)
                    isItemchanged = True
                End If
            ElseIf aItem.DATATYPE = otFieldDataType.[Long] And IsNumeric(Value) Then
                If aItem.Value <> CLng(Value) Then
                    aItem.Value = CLng(Value)
                    isItemchanged = True
                End If
            ElseIf aItem.DATATYPE = otFieldDataType.Bool Then
                If aItem.Value <> CBool(Value) Then
                    aItem.Value = CBool(Value)
                    isItemchanged = True
                End If
            Else
                If aItem.Value <> CStr(Value) Then
                    aItem.Value = CStr(Value)
                    isItemchanged = True
                End If
            End If
            ' save it to dictionary
            ' get Item
            If isItemchanged Then
                'Call s_items.add(Key:=LCase(aRealID), Item:=aItem) -> should be ok since referenced
                s_isItemChanged = True

                setItem = True
                Exit Function
            Else
                setItem = True
                Exit Function
            End If


            '
            setItem = False

        End Function

        '********** static createSchema
        '**********
        Public Function createSchema(Optional silent As Boolean = True) As Boolean
'            Dim aFieldDesc As New ormFieldDescription
'            Dim PrimaryColumnNames As New Collection
'            Dim WorkspaceColumnNames As New Collection
'            Dim CompundIndexColumnNames As New Collection
'            Dim aTable As New ObjectDefinition
'            Dim aTableEntry As New ObjectEntryDefinition


'            aFieldDesc.ID = ""
'            aFieldDesc.Parameter = ""
'            aFieldDesc.Relation = New String() {}
'            aFieldDesc.Aliases = New String() {}
'            aFieldDesc.Tablename = ourTableName

'            ' delete just fields -> keep compounds
'            If aTable.Inject(ourTableName) Then
'                For Each aTableEntry In aTable.Entries
'                    If aTableEntry.Typeid = otObjectEntryDefinitiontype.Field Then
'                        aTableEntry.Delete()
'                    End If
'                Next aTableEntry
'                aTable.Persist()
'            End If
'            aTable = New ObjectDefinition
'            aTable.Create(ourTableName)

'            '******
'            '****** Fields

'            With aTable


'                '**** UID
'                aFieldDesc.Datatype = otFieldDataType.[Long]
'                aFieldDesc.Title = "uid of configuration"
'                aFieldDesc.ID = "cnf1"
'                aFieldDesc.Aliases = New String() {}
'                aFieldDesc.ColumnName = "uid"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
'                CompundIndexColumnNames.Add(aFieldDesc.ColumnName)
'                '**** msglogtag
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "configuration name"
'                aFieldDesc.Aliases = New String() {}
'                aFieldDesc.ID = "cnf2"
'                aFieldDesc.ColumnName = "cname"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                CompundIndexColumnNames.Add(aFieldDesc.ColumnName)
'                '***** updc
'                aFieldDesc.Datatype = otFieldDataType.[Long]
'                aFieldDesc.Title = "update count"
'                aFieldDesc.ID = "cnf3"
'                aFieldDesc.Aliases = New String() {}
'                aFieldDesc.ColumnName = "updc"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                CompundIndexColumnNames.Add(aFieldDesc.ColumnName)

'                '**** msglogtag
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "message log tag"
'                aFieldDesc.Aliases = New String() {}
'                aFieldDesc.ID = "cnf20"
'                aFieldDesc.ColumnName = "msglogtag"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)


'                '**** comment
'                aFieldDesc.Datatype = otFieldDataType.Memo
'                aFieldDesc.Title = "comment"
'                aFieldDesc.Aliases = New String() {}
'                aFieldDesc.ID = "cnf30"
'                aFieldDesc.ColumnName = "cmt"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                '***
'                '*** TIMESTAMP
'                '****
'                aFieldDesc.Datatype = otFieldDataType.Timestamp
'                aFieldDesc.Title = "last Update"
'                aFieldDesc.ColumnName = ConstFNUpdatedOn
'                aFieldDesc.ID = ""
'                aFieldDesc.Aliases = New String() {}
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                aFieldDesc.Datatype = otFieldDataType.Timestamp
'                aFieldDesc.Title = "creation Date"
'                aFieldDesc.ColumnName = ConstFNCreatedOn
'                aFieldDesc.ID = ""
'                aFieldDesc.Aliases = New String() {}
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                ' Index
'                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
'                Call .AddIndex("workspaceID", WorkspaceColumnNames, isprimarykey:=False)
'                Call .AddIndex(ConstDefaultCompoundIndexName, CompundIndexColumnNames, isprimarykey:=False)
'                ' persist
'                .Persist()
'                ' change the database
'                .CreateObjectSchema()
'            End With

'            ' reset the Table description
'            If Not Me.Record.SetTable(ourTableName, forceReload:=True) Then
'                Call CoreMessageHandler(subname:="clsOTDBConfigurable.createSchema", tablename:=ourTableName, _
'                                      message:="Error while setTable in createSchema")
'            End If

'            '
'            createSchema = True
'            Exit Function

'            ' Handle the error
'error_handle:
'            Call CoreMessageHandler(subname:="clsOTDBConfigurable.createSchema", tablename:=ourTableName)
'            createSchema = False
        End Function

        '***** loadItems -> load all Items as Items
        '*****
        Public Function loadItems(ByVal CONFIGNAME As String) As Boolean
            Dim aTable As iormDataStore
            Dim aVAlue As Object
            Dim anItem As New clsOTDBConfigurableItem
            Dim aDefConfig As New clsOTDBDefConfiguration
            Dim aDefConfigItem As New clsOTDBDefConfigurationItem
            Dim aCollection As New Collection
            Dim updc As Long
            Dim isCache As Boolean
            Dim m As Object

            aTable = GetTableStore(ourTableName)
            If Not aDefConfig.Inject(CONFIGNAME:=CONFIGNAME) Then
                loadItems = False
                Exit Function
            End If

            For Each m In aDefConfig.Items
                aDefConfigItem = m
                ' create the ITEM or load it
                If aTable.TableSchema.Hasfieldname(aDefConfigItem.ID) Then
                    aVAlue = Me.Record.GetValue(aDefConfigItem.ID)
                Else
                    aVAlue = Null()
                End If
                '** create ITEM
                If anItem.create(UID:=s_uid, CONFIGNAME:=s_configname, ID:=aDefConfigItem.ID) Then
                    anItem.DATATYPE = aDefConfigItem.DATATYPE
                    'anItem.isCacheNoSave = isCache
                    anItem.cmt = aDefConfigItem.COMMENT
                    ' set the value
                    If Not IsNull(aVAlue) Then
                        anItem.Value = aVAlue
                    Else
                        ' reset if no value
                        If anItem.DATATYPE = otFieldDataType.[Date] _
                           Or anItem.DATATYPE = otFieldDataType.Timestamp Then
                            anItem.Value = ConstNullDate
                        ElseIf anItem.DATATYPE = otFieldDataType.Text _
                               Or anItem.DATATYPE = otFieldDataType.List Then
                            anItem.Value = ""
                        Else
                            anItem.Value = 0
                        End If
                    End If
                    Call anItem.PERSIST()
                Else
                    Call anItem.Inject(UID:=s_uid, CONFIGNAME:=s_configname, ID:=aDefConfigItem.ID)
                End If
                '** include
                Call addItem(ITEM:=anItem)


            Next m

            loadItems = True
        End Function

        '**** infuse the the Object by a OTBRecord
        '****
        Public Function infuse(ByRef aRecord As ormRecord) As Boolean
            Dim aTable As iormDataStore
            Dim i As Integer
            Dim fieldname As String
            Dim aVAlue As Object



            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    infuse = False
                    Exit Function
                End If
            End If

            '*** overload it from the Application Container
            '***
            'If Me.serializeWithHostApplication Then
            '    If overloadFromHostApplication(aRecord) Then
            '        s_loadedFromHost = True
            '    End If
            'End If

            On Error GoTo errorhandle

            Me.Record = aRecord
            _IsLoaded = True
            s_uid = CLng(aRecord.GetValue("uid"))
            s_configname = CStr(aRecord.GetValue("cname"))
            s_updc = CLng(aRecord.GetValue("updc"))
            s_msglogtag = CStr(aRecord.GetValue("msglogtag"))
            s_comment = CStr(aRecord.GetValue("cmt"))

            _updatedOn = CDate(aRecord.GetValue(ConstFNUpdatedOn))

            ' is loaded
            s_isItemChanged = False
            _IsLoaded = True

            '*** fill the ITEM Dictionary
            If Not loadItems(CONFIGNAME:=s_configname) Then
                Me.Unload()
                infuse = False
                Exit Function
            End If

            infuse = MyBase.Infuse(aRecord)

            Exit Function

errorhandle:
            Me.Unload()
            infuse = False


        End Function
        '** set the serialize with HostApplication
        'Public Property Get serializeWithHostApplication() As Boolean
        '    serializeWithHostApplication = s_serializeWithHostApplication
        'End Property
        'Public Property Let serializeWithHostApplication(aValue As Boolean)
        '    If aValue Then
        '        If isRegisteredAtHostApplication(ourTableName) Then
        '            s_serializeWithHostApplication = True
        '        Else
        '            s_serializeWithHostApplication = registerHostApplicationFor(ourTableName, AllObjectSerialize:=False)
        '        End If
        '    Else
        '        s_serializeWithHostApplication = False
        '    End If
        'End Property

        '**** delete
        '****
        Public Function delete() As Boolean
            Dim anEntry As New clsOTDBConfigurableItem
            Dim m As Object

            If IsLoaded Then
                ' delete each entry
                For Each kvp As KeyValuePair(Of String, clsOTDBConfigurableItem) In s_items
                    anEntry = kvp.Value
                    anEntry.Delete()
                Next
                Me.IsDeleted = Me.Record.Delete()
                If Me.IsDeleted Then
                    Me.Unload()
                End If
                delete = Me.IsDeleted
                Exit Function
            Else
                delete = False
            End If
        End Function
        '**** Items returns a Collection of Items for CONFIGNAME-list
        '****
        Public Function Items() As Collection
            Dim anEntry As New clsOTDBConfigurableItem
            Dim aCollection As New Collection
            Dim m As Object

            ' delete each entry
            For Each kvp As KeyValuePair(Of String, clsOTDBConfigurableItem) In s_items
                If Not isNothing(m) Then
                    anEntry = kvp.Value
                    Call aCollection.Add(Item:=anEntry)
                End If
            Next

            Items = aCollection
        End Function
        '*** add a Component by cls OTDB
        '***
        Public Function addItem(ByRef ITEM As clsOTDBConfigurableItem) As Boolean
            Dim flag As Boolean

            Dim m As Object

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                addItem = False
                Exit Function
            End If

            ' remove and overwrite
            If s_items.ContainsKey(key:=ITEM.ID) Then
                Call s_items.Remove(key:=ITEM.ID)
            End If

            If s_orgItemValues.ContainsKey(key:=ITEM.ID) Then
                Call s_orgItemValues.Remove(key:=ITEM.ID)
            End If

            ' add Item Entry
            s_items.Add(key:=ITEM.ID, value:=ITEM)
            ' copy
            Call s_orgItemValues.Add(key:=ITEM.ID, value:=ITEM.Value)

            '
            addItem = True

        End Function


        '**** Inject : load the object by the PrimaryKeys
        '****
        Public Function Inject(UID As Long) As Boolean
            Dim aTable As iormDataStore
            Dim pkarry() As Object
            Dim aRecord As ormRecord
            Dim aRecordCollection As List(Of ormRecord)

            '* init
            If Not Me.IsInitialized Then
                If Not initialize() Then
                    Inject = False
                    Exit Function
                End If
            End If

            ' set the primaryKey
            ReDim pkarry(1)
            pkarry(0) = UID


            aTable = GetTableStore(Me.Record.TableID)
            aRecord = aTable.GetRecordByPrimaryKey(pkarry)

            If aRecord Is Nothing Then
                Me.Unload()
                Inject = Me.IsLoaded
                Exit Function
            Else
                Me.Record = aRecord
                _IsLoaded = Me.infuse(Me.Record)
                If Not _IsLoaded Then
                    Inject = False
                    Exit Function
                End If

                Inject = MyBase.Infuse(aRecord)
                Exit Function
            End If

        End Function

        '**** create : create the object by the PrimaryKeys
        '****
        Public Function create(ByVal UID As Long, _
                               Optional ByVal CONFIGNAME As String = "") As Boolean
            Dim aTable As iormDataStore
            Dim pkarry() As Object
            Dim aRecord As ormRecord

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    create = False
                    Exit Function
                End If
            End If

            If IsLoaded Then
                create = False
                Exit Function
            End If

            ' set the primaryKey
            ReDim pkarry(1)
            pkarry(0) = UID


            aTable = GetTableStore(Me.Record.TableID)
            aRecord = aTable.GetRecordByPrimaryKey(pkarry)

            If Not aRecord Is Nothing Then
                create = False
                Exit Function
            Else
                ' set the primaryKey
                s_uid = UID

                _IsCreated = True    ' here we are create

                ' this will set also the loadItems
                If Not IsMissing(CONFIGNAME) Then
                    s_configname = CONFIGNAME
                    Call loadItems(CONFIGNAME:=CONFIGNAME)
                End If

                create = Me.IsCreated
                Exit Function
            End If

        End Function

        '******* existsItem: checks if the ITEM by ID exists and is Of Type
        '*******
        Public Function existsItem(ByVal ID As String, _
                                   Optional ByVal HASDATA As Boolean = True) As Boolean
            Dim aVAlue As Object
            Dim aDefConfiguration As clsOTDBDefConfiguration
            Dim aRealID As String
            Dim aDefConfigItem As New clsOTDBDefConfigurationItem
            Dim anItem As clsOTDBConfigurableItem


            If Not IsCreated And Not IsLoaded Then
                existsItem = False
                Exit Function
            End If

            ' check aliases
            aDefConfiguration = Me.getDefConfiguration
            If aDefConfiguration Is Nothing Then
                Call CoreMessageHandler(message:="DefConfiguration is not valid", arg1:=Me.CONFIGNAME, subname:="clsOTDBConfigurable.getItem")
                existsItem = False
                Exit Function

            End If
            aRealID = aDefConfiguration.getIDByAlias(AliasID:=LCase(ID))
            If aRealID = "" Then
                aRealID = LCase(ID)
            End If
            ' get the DefConfiguration ITEM
            'aDefConfigItem = aDefConfigItem.Singleton(Me.CONFIGNAME, ID:=aRealID)


            ' if ITEM exists in Items
            If s_items.ContainsKey(LCase(aRealID)) Then
                anItem = s_items.Item(LCase(aRealID))
                aVAlue = anItem.Value
                Select Case anItem.DATATYPE

                    Case otFieldDataType.Numeric, otFieldDataType.[Long]
                        If IsNumeric(aVAlue) And HASDATA Then
                            existsItem = True
                        ElseIf Not IsNumeric(aVAlue) Then
                            existsItem = False
                        Else
                            existsItem = True
                        End If
                    Case otFieldDataType.List, otFieldDataType.Text, otFieldDataType.Memo

                        If Trim(CStr(aVAlue)) <> "" And HASDATA Then
                            existsItem = True

                        ElseIf Trim(CStr(aVAlue)) = "" And HASDATA Then
                            existsItem = False
                        ElseIf Not HASDATA Then
                            existsItem = True
                        Else
                            existsItem = True
                        End If

                    Case otFieldDataType.Formula, otFieldDataType.Binary, otFieldDataType.Runtime
                        existsItem = True
                    Case otFieldDataType.[Date], otFieldDataType.Timestamp
                        If IsDate(aVAlue) Then
                            If HASDATA And aVAlue <> ConstNullDate Then
                                existsItem = True
                            ElseIf Not HASDATA Then
                                existsItem = True
                            Else
                                existsItem = False
                            End If
                        ElseIf Not HASDATA Then
                            existsItem = True
                        Else
                            existsItem = False
                        End If
                    Case otFieldDataType.Bool
                        existsItem = True

                End Select



            Else
                existsItem = False
                Exit Function
            End If

        End Function
        '******* checks if ITEM has data
        '*******
        Public Function isNothing(ByVal ID As String, Optional ByVal ifNotExists As Boolean = False) As Boolean
            Dim aVAlue As Object

            aVAlue = Me.existsItem(ID:=ID)
            If Not aVAlue Then
                isNothing = ifNotExists
            Else
                isNothing = False
            End If

        End Function

        '**** updateRecord
        '****
        Public Function updateRecord() As Boolean
            Dim aTable As iormDataStore
            Dim i As Integer
            Dim fieldname As String
            Dim aVAlue As Object

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    updateRecord = False
                    Exit Function
                End If
            End If
            If Not IsLoaded And Not IsCreated Then
                updateRecord = False
                Exit Function
            End If


            'On Error GoTo errorhandle
            Call Me.Record.SetValue("uid", s_uid)
            Call Me.Record.SetValue("cmt", s_comment)
            Call Me.Record.SetValue("updc", s_updc)
            Call Me.Record.SetValue("cname", s_configname)

            updateRecord = True
        End Function
        '**** persist
        '****

        Public Function PERSIST(Optional ByVal TIMESTAMP As Date = Nothing, _
                            Optional ByVal ForceSerializeToOTDB As Boolean = False) As Boolean
            Dim anItem As New clsOTDBConfigurableItem
            Dim m As Object

            If Not updateRecord() Then
                PERSIST = False
                Exit Function
            End If

            '*** overload it from the Application Container
            '***
            'If Me.serializeWithHostApplication Then
            '    If overwriteToHostApplication(me.record) Then
            '        s_savedToHost = True
            '    End If
            'End If
            If IsMissing(TIMESTAMP) Or Not IsDate(TIMESTAMP) Then
                TIMESTAMP = Now
            End If
            'If ForceSerializeToOTDB Or Not Me.serializeWithHostApplication Then
            ' persist all the milestones
            For Each kvp As KeyValuePair(Of String, clsOTDBConfigurableItem) In s_items
                anItem = kvp.Value

                Call anItem.PERSIST(TIMESTAMP)
            Next

            PERSIST = Me.Record.Persist(TIMESTAMP)

            'End If

            ' reset change flags
            If PERSIST Then

                s_isItemChanged = False
            End If

            Exit Function

errorhandle:

            PERSIST = False

        End Function

        '**** clone the object
        '****
        Public Function clone(ByVal UID As Long) As clsOTDBConfigurable
            '*** now we copy the object
            Dim aNewObject As New clsOTDBConfigurable
            Dim newRecord As New ormRecord

            Dim aItem As clsOTDBConfigurableItem
            Dim aCloneItem As clsOTDBConfigurableItem

            Dim m As Object
            Dim aVAlue As Object

            If Not IsLoaded And Not IsCreated Then
                clone = Nothing
                Exit Function
            End If
            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    clone = Nothing
                    Exit Function
                End If
            End If

            If Not updateRecord() Then
                clone = Nothing
                Exit Function
            End If

            If Not aNewObject.create(UID:=UID, CONFIGNAME:=s_configname) Then
                clone = Nothing
                Exit Function
            End If

            ' set it
            newRecord.SetTable(Me.TableID)

            ' go through the table and overwrite the Record if the rights are there
            For Each m In Me.Record.Keys
                If m <> ConstFNCreatedOn And m <> ConstFNUpdatedOn Then
                    Call newRecord.SetValue(m, Me.Record.GetValue(m))
                End If
            Next m

            ' overwrite the primary keys
            Call newRecord.SetValue("uid", UID)
            'Call newRecord.setValue("updc", updc)

            ' actually here it we should clone all Items too !

            If aNewObject.infuse(newRecord) Then
                ' now clone the Items (Milestones)
                For Each kvp As KeyValuePair(Of String, clsOTDBConfigurableItem) In s_items
                    aItem = kvp.Value
                    aCloneItem = aItem.clone(UID:=UID, CONFIGNAME:=Me.CONFIGNAME, ID:=aItem.ID)
                    If Not aCloneItem Is Nothing Then
                        Call aNewObject.addItem(ITEM:=aCloneItem)
                    End If
                Next
                clone = aNewObject
            Else
                clone = Nothing
            End If
        End Function

        '**** getNewUID
        '****
        Public Function getNewUID(ByRef newUID As Long) As Boolean
            Dim otdbcn As ADODB.Connection
            Dim rst As ADODB.Recordset
            Dim tablename As String
            Dim cmdstr As String
            Dim mynewUID As Long

            Dim i As Integer
            Dim j As Integer


            ' Connection
            '** TODO !
            'otdbcn = ADOConnection()
            If otdbcn Is Nothing Then
                getNewUID = False
                Exit Function
            End If

            On Error GoTo error_handle
            rst = New ADODB.Recordset

            ' get

            cmdstr = "SELECT max(UID) from " & Me.TableID & " where uid > 0"


            rst.Open(cmdstr, otdbcn, ADODB.CursorTypeEnum.adOpenStatic, ADODB.LockTypeEnum.adLockOptimistic)
            If Not rst.EOF Then
                If Not IsNull(rst.Fields(0).Value) And IsNumeric(rst.Fields(0).Value) Then
                    mynewUID = CLng(rst.Fields(0).Value)
                Else
                    mynewUID = 0
                End If
                getNewUID = True

            Else
                getNewUID = False
            End If

            ' close
            rst.Close()


            newUID = mynewUID + 1
            '*


            Exit Function

            ' Handle the error
error_handle:
            Call CoreMessageHandler(showmsgbox:=False, subname:="clsOTDBConfigurable.getNewUID")
            getNewUID = False
        End Function

        ''' <summary>
        ''' run the XPrecheck on the Envelope and Object
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunXPreCheck(ByRef envelope As XEnvelope) As Boolean Implements iotXChangeable.RunXPreCheck

        End Function
        ''' <summary>
        ''' run the XChange with the envelope
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunXChange(ByRef envelope As XEnvelope) As Boolean Implements iotXChangeable.RunXChange

        End Function
        '***** runXChange runs the eXChange for the class
        '*****
        Public Function runXChangeOLD(ByRef MAPPING As Dictionary(Of Object, Object), _
                                                ByRef CHANGECONFIG As clsOTDBXChangeConfig, _
                                                Optional ByRef MSGLOG As ObjectLog = Nothing) As Boolean
            Dim aCMuid As clsOTDBXChangeMember
            Dim aCMupdc As clsOTDBXChangeMember
            Dim aCMWspace As clsOTDBXChangeMember
            Dim aChangeItem As New clsOTDBXChangeMember

            Dim anUID As Long
            Dim anUPDC As Long
            Dim aNewUPDC As Long
            Dim aCollection As New Collection
            Dim aFlag As Boolean

            Dim aSchedule As New Schedule
            Dim aCurrSCHEDULE As New CurrentSchedule
            Dim aDeliverable As New Deliverable
            Dim aTrack As New Track
            Dim anObjectDef As New clsOTDBXChangeMember
            Dim anAttribute As New clsOTDBXChangeMember
            Dim aNewSchedule As New Schedule
            Dim aWorkspace As String
            Dim setCurrSchedule As Boolean
            Dim aVAlue As Object

            Dim aTimestamp As Date

            If CHANGECONFIG.ProcessedDate <> ConstNullDate Then
                aTimestamp = CHANGECONFIG.ProcessedDate
            Else
                aTimestamp = Now
            End If

            '*** ObjectDefinition
            anObjectDef = CHANGECONFIG.ObjectByName(ourTableName)

            ' set msglog
            If MSGLOG Is Nothing Then
                If s_msglog Is Nothing Then
                    s_msglog = New ObjectLog
                End If
                MSGLOG = s_msglog
                MSGLOG.Create(Me.msglogtag)
            End If


            '** check on the min. required primary key uid
            aVAlue = CHANGECONFIG.GetMemberValue(ID:="SC2", mapping:=MAPPING)
            If IsNull(aVAlue) Then
                ' error condition
                aCMuid = CHANGECONFIG.AttributeByID("SC2")
                If aCMuid Is Nothing Then
                    Call MSGLOG.AddMsg("200", Nothing, Nothing, "SC2", "SC2", ourTableName, CHANGECONFIG.Configname)
                    runXChangeOLD = False
                    Exit Function
                Else
                    Call MSGLOG.AddMsg("201", Nothing, Nothing, "SC2", "SC2", ourTableName, CHANGECONFIG.Configname)
                    runXChangeOLD = False
                    Exit Function
                End If
                '**
            ElseIf Not IsNumeric(aVAlue) Then
                Call MSGLOG.AddMsg("202", Nothing, Nothing, "SC2", "SC2", ourTableName, CHANGECONFIG.Configname, aVAlue, "numeric")
                runXChangeOLD = False
                Exit Function
            Else
                anUID = CLng(aVAlue)
            End If




            runXChangeOLD = True
        End Function

        '***** runXPreCheck runs the precheck
        '*****
        Public Function runXPreCheckOLD(ByRef MAPPING As Dictionary(Of Object, Object), _
                                                  ByRef CHANGECONFIG As clsOTDBXChangeConfig, _
                                                  Optional ByRef MSGLOG As ObjectLog = Nothing) As Boolean
            Dim aCMuid As clsOTDBXChangeMember
            Dim aCMupdc As clsOTDBXChangeMember
            Dim anObject As New clsOTDBXChangeMember
            Dim aVAlue As Object
            Dim anUPDC As Long
            Dim anUID As Long

            ' set msglog
            If MSGLOG Is Nothing Then
                MSGLOG = s_msglog
                MSGLOG.Create(s_msglogtag)
            End If
            '** check on the min. required primary key uid
            aVAlue = CHANGECONFIG.GetMemberValue(ID:="SC2", mapping:=MAPPING)
            If IsNull(aVAlue) Then
                ' error condition
                aCMuid = CHANGECONFIG.AttributeByID("SC2")
                If aCMuid Is Nothing Then
                    Call MSGLOG.AddMsg("200", Nothing, Nothing, "SC2", "SC2", ourTableName, CHANGECONFIG.Configname)
                    runXPreCheckOLD = False
                    Exit Function
                Else
                    Call MSGLOG.AddMsg("201", Nothing, Nothing, "SC2", "SC2", ourTableName, CHANGECONFIG.Configname)
                    runXPreCheckOLD = False
                    Exit Function
                End If
                '**
            ElseIf Not IsNumeric(aVAlue) Then
                Call MSGLOG.AddMsg("202", Nothing, Nothing, "SC2", "SC2", ourTableName, CHANGECONFIG.Configname, aVAlue, "numeric")
                runXPreCheckOLD = False
                Exit Function
            Else
                anUID = CLng(aVAlue)
            End If


            ' optional key updc
            aVAlue = CHANGECONFIG.GetMemberValue(ID:="SC3", mapping:=MAPPING)
            If IsNull(aVAlue) Then
                'Call msglog.addMsg("201", Nothing, Nothing, "SC3", "SC3", ourTableName, ChangeConfig.ConfigName)
                anUPDC = -1
            ElseIf Not IsNumeric(aVAlue) Then
                anUPDC = -1
            Else
                anUPDC = CLng(aVAlue)

            End If

            ' generell tests
            anObject = CHANGECONFIG.ObjectByName(Me.TableID)
            runXPreCheckOLD = CHANGECONFIG.runDefaultXPreCheck(anObject:=anObject, _
                                                                         aMapping:=MAPPING, MSGLOG:=MSGLOG)


        End Function


    End Class
    '************************************************************************************
    '***** CLASS clsOTDBConfigurableItem describes additional database schema information
    '*****
    Class clsOTDBConfigurableItem

        Inherits ormDataObject
        Const ourTableName = "tblConfigItems"

        'Private s_serializeWithHostApplication As Boolean
        ' fields

        Private s_uid As Long
        Private s_configname As String
        Private s_id As String

        Private s_value As Object
        Private s_datatype As otFieldDataType
        Private s_cmt As String

        Private s_msglogtag As String

        Private s_parameter_txt1 As String
        Private s_parameter_txt2 As String
        Private s_parameter_txt3 As String
        Private s_parameter_num1 As Double
        Private s_parameter_num2 As Double
        Private s_parameter_num3 As Double
        Private s_parameter_date1 As Date
        Private s_parameter_date2 As Date
        Private s_parameter_date3 As Date
        Private s_parameter_flag1 As Boolean
        Private s_parameter_flag2 As Boolean
        Private s_parameter_flag3 As Boolean



        'dynamic
        'Private s_loadedFromHost As Boolean
        'Private s_savedToHost As Boolean
        Private s_isCacheNoSave As Boolean    ' if set this is not saved since taken from another configname
        Private s_msglog As New ObjectLog

        '** initialize
        Public Sub New()
            MyBase.New(ourTableName)

        End Sub



        '****** getUniqueTag
        Public Function getUniqueTag()
            getUniqueTag = ConstDelimiter & ourTableName & ConstDelimiter & s_uid & ConstDelimiter & s_configname & ConstDelimiter & s_id & ConstDelimiter
        End Function
        ReadOnly Property msglogtag() As String
            Get
                If s_msglogtag = "" Then
                    s_msglogtag = getUniqueTag()
                End If
                msglogtag = s_msglogtag
            End Get
        End Property

        Public Property UID() As Long

            Get

                UID = s_uid
            End Get
            Set(value As Long)
                If s_uid <> value Then
                    s_uid = value
                    Me.IsChanged = True
                End If
            End Set

        End Property

        Public Property CONFIGNAME() As String
            Get
                CONFIGNAME = s_configname
            End Get
            Set(value As String)
                If LCase(s_configname) <> LCase(value) Then
                    s_configname = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property ID() As String
            Get
                ID = s_id
            End Get
            Set(value As String)
                If LCase(s_id) <> LCase(value) Then
                    s_id = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Value() As Object
            Get
                Value = s_value
            End Get
            Set(ByVal value As Object)
                If value <> s_value Then
                    s_value = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property DATATYPE() As otFieldDataType
            Get
                DATATYPE = s_datatype
            End Get
            Set(value As otFieldDataType)
                If s_datatype <> value Then
                    s_datatype = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property cmt() As String
            Get
                cmt = s_cmt
            End Get
            Set(value As String)
                If LCase(s_cmt) <> LCase(value) Then
                    s_cmt = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property isCacheNoSave() As Boolean
            Get
                isCacheNoSave = s_isCacheNoSave
            End Get
            Set(value As Boolean)
                If value Then
                    s_isCacheNoSave = True
                Else
                    s_isCacheNoSave = False
                End If
            End Set
        End Property

        Public Property parameter_num1() As Double
            Get
                parameter_num1 = s_parameter_num1
            End Get
            Set(value As Double)
                If s_parameter_num1 <> value Then
                    s_parameter_num1 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_num2() As Double
            Get
                parameter_num2 = s_parameter_num2
            End Get
            Set(value As Double)
                If s_parameter_num2 <> value Then
                    s_parameter_num2 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_num3() As Double
            Get
                parameter_num3 = s_parameter_num3
            End Get
            Set(value As Double)
                If s_parameter_num3 <> value Then
                    s_parameter_num3 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_date1() As Date
            Get
                parameter_date1 = s_parameter_date1
            End Get
            Set(value As Date)
                If s_parameter_date1 <> value Then
                    s_parameter_date1 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_date2() As Date
            Get
                parameter_date2 = s_parameter_date2
            End Get
            Set(value As Date)
                If s_parameter_date2 <> value Then
                    s_parameter_date2 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_date3() As Date
            Get
                parameter_date3 = s_parameter_date3
            End Get
            Set(value As Date)
                s_parameter_date3 = value
                Me.IsChanged = True
            End Set
        End Property
        Public Property parameter_flag1() As Boolean
            Get
                parameter_flag1 = s_parameter_flag1
            End Get
            Set(value As Boolean)
                If s_parameter_flag1 <> value Then
                    s_parameter_flag1 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_flag3() As Boolean
            Get
                parameter_flag3 = s_parameter_flag3
            End Get
            Set(value As Boolean)
                If s_parameter_flag3 <> value Then
                    s_parameter_flag3 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_flag2() As Boolean
            Get
                parameter_flag2 = s_parameter_flag2
            End Get
            Set(value As Boolean)
                If s_parameter_flag2 <> value Then
                    s_parameter_flag2 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_txt1() As String
            Get
                parameter_txt1 = s_parameter_txt1
            End Get
            Set(value As String)
                If s_parameter_txt1 <> value Then
                    s_parameter_txt1 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_txt2() As String
            Get
                parameter_txt2 = s_parameter_txt2
            End Get
            Set(value As String)
                If s_parameter_txt2 <> value Then
                    s_parameter_txt2 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_txt3() As String
            Get
                parameter_txt3 = s_parameter_txt3
            End Get
            Set(value As String)
                If s_parameter_txt3 <> value Then
                    s_parameter_txt3 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        '*** init
        Public Function initialize() As Boolean
            initialize = MyBase.Initialize
            's_serializeWithHostApplication = isDefaultSerializeAtHostApplication(ourTableName)
            ' s_workspace =CurrentSession.DefaultWorkspace
            's_valuedate = ot.ConstNullDate
            s_parameter_date1 = ConstNullDate
            s_parameter_date2 = ConstNullDate
            s_parameter_date3 = ConstNullDate


        End Function
        '** set the serialize with HostApplication
        'Public Property Get serializeWithHostApplication() As Boolean
        '    serializeWithHostApplication = s_serializeWithHostApplication
        'End Property
        'Public Property Let serializeWithHostApplication(aValue As Boolean)
        '    If aValue Then
        '        If isRegisteredAtHostApplication(ourTableName) Then
        '            s_serializeWithHostApplication = True
        '        Else
        '            s_serializeWithHostApplication = registerHostApplicationFor(ourTableName, AllObjectSerialize:=False)
        '        End If
        '    Else
        '        s_serializeWithHostApplication = False
        '    End If
        'End Property


        '**** infuese the object by a OTDBRecord
        '****
        Public Function infuse(ByRef aRecord As ormRecord) As Boolean
            Dim aVAlue As Object

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    infuse = False
                    Exit Function
                End If
            End If


            '*** overload it from the Application Container
            '***
            'If Me.serializeWithHostApplication Then
            '    If overloadFromHostApplication(aRecord) Then
            '        s_loadedFromHost = True
            '    End If
            'End If

            On Error GoTo errorhandle

            Me.Record = aRecord
            s_uid = CStr(aRecord.GetValue("uid"))
            s_configname = CStr(aRecord.GetValue("cname"))
            s_id = CStr(aRecord.GetValue("id"))

            s_datatype = CLng(aRecord.GetValue("datatype"))
            aVAlue = aRecord.GetValue("value")
            ' select on Datatype
            Select Case s_datatype

                Case otFieldDataType.Numeric
                    s_value = CDbl(aVAlue)
                Case otFieldDataType.List, otFieldDataType.Text
                    s_value = CStr(aVAlue)
                Case otFieldDataType.Runtime, otFieldDataType.Formula, otFieldDataType.Binary
                    s_value = ""
                    Call CoreMessageHandler(subname:="clsOTDBConfigurableItem.infuse", _
                                          message:="runtime, formular, binary can't infuse", msglog:=s_msglog, arg1:=aVAlue)
                Case otFieldDataType.[Date], otFieldDataType.Timestamp
                    s_value = CDate(aVAlue)
                Case otFieldDataType.[Long]
                    s_value = CLng(aVAlue)
                Case otFieldDataType.Bool
                    s_value = CBool(aVAlue)
                Case otFieldDataType.Memo
                    s_value = CStr(aVAlue)
                Case Else
                    Call CoreMessageHandler(subname:="clsOTDBConfigurableItem.infuse", _
                                          message:="unknown datatype to be infused", msglog:=s_msglog, arg1:=aVAlue)
            End Select


            s_cmt = CStr(aRecord.GetValue("cmt"))

            s_msglogtag = CStr(aRecord.GetValue("msglogtag"))

            s_parameter_txt1 = CStr(aRecord.GetValue("param_txt1"))
            s_parameter_txt2 = CStr(aRecord.GetValue("param_txt2"))
            s_parameter_txt3 = CStr(aRecord.GetValue("param_txt3"))
            s_parameter_num1 = CDbl(aRecord.GetValue("param_num1"))
            s_parameter_num2 = CDbl(aRecord.GetValue("param_num2"))
            s_parameter_num3 = CDbl(aRecord.GetValue("param_num3"))
            s_parameter_date1 = CDate(aRecord.GetValue("param_date1"))
            s_parameter_date2 = CDate(aRecord.GetValue("param_date2"))
            s_parameter_date3 = CDate(aRecord.GetValue("param_date3"))
            s_parameter_flag1 = CBool(aRecord.GetValue("param_flag1"))
            s_parameter_flag2 = CBool(aRecord.GetValue("param_flag2"))
            s_parameter_flag3 = CBool(aRecord.GetValue("param_flag3"))

            _updatedOn = CDate(aRecord.GetValue(ConstFNUpdatedOn))
            _createdOn = CDate(aRecord.GetValue(ConstFNCreatedOn))


            _IsLoaded = MyBase.Infuse(aRecord)
            infuse = Me.IsLoaded
            Exit Function

errorhandle:
            infuse = False


        End Function


        '**** Inject : load the object by the PrimaryKeys
        '****
        Public Function Inject(ByVal UID As Long, ByVal CONFIGNAME As String, ByVal ID As String) As Boolean
            Dim aTable As iormDataStore
            Dim pkarry(3) As Object
            Dim aRecord As ormRecord

            '* lazy init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    Inject = False
                    Exit Function
                End If
            End If

            ' set the primaryKey
            pkarry(0) = UID
            pkarry(1) = LCase(CONFIGNAME)
            pkarry(2) = LCase(ID)


            aTable = GetTableStore(ourTableName)
            aRecord = aTable.GetRecordByPrimaryKey(pkarry)

            If aRecord Is Nothing Then
                Me.Unload()
                Inject = Me.IsLoaded
                Exit Function
            Else
                Me.Record = aRecord
                _IsLoaded = Me.infuse(Me.Record)
                Inject = Me.IsLoaded
                Exit Function
            End If


        End Function
        '********** static createSchema
        '********** create the Schema for the Directory to enable bootstrapping provide the Connection to be used
        '**********
        Public Function createSchema(Optional silent As Boolean = True) As Boolean

'            Dim aFieldDesc As New ormFieldDescription
'            Dim PrimaryColumnNames As New Collection
'            Dim CompoundIndexColumnNames As New Collection
'            Dim aTable As New ObjectDefinition


'            aFieldDesc.ID = ""
'            aFieldDesc.Parameter = ""
'            aFieldDesc.Tablename = ourTableName

'            With aTable
'                .Create(ourTableName)
'                .Delete()

'                '***
'                '*** Fields
'                '****

'                'Type
'                aFieldDesc.Datatype = otFieldDataType.[Long]

'                aFieldDesc.Title = "uid of the configuration"
'                aFieldDesc.ColumnName = "uid"
'                aFieldDesc.ID = "cfi1"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
'                CompoundIndexColumnNames.Add(aFieldDesc.ColumnName)
'                'configname
'                aFieldDesc.Datatype = otFieldDataType.[Long]
'                aFieldDesc.Title = "configname of config"
'                aFieldDesc.ColumnName = "cname"
'                aFieldDesc.ID = "cfi2"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
'                CompoundIndexColumnNames.Add(aFieldDesc.ColumnName)

'                'id
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "config item id"
'                aFieldDesc.ColumnName = "id"
'                aFieldDesc.ID = "cfi3"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

'                'value
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "value as text"
'                aFieldDesc.ColumnName = "value"
'                aFieldDesc.ID = "cfi4"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                'date
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "value as date"
'                aFieldDesc.ColumnName = "valuedate"
'                aFieldDesc.ID = "cfi5"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                'numeric
'                aFieldDesc.Datatype = otFieldDataType.Numeric
'                aFieldDesc.Title = "value as numeric"
'                aFieldDesc.ColumnName = "valuenumeric"
'                aFieldDesc.ID = "cfi6"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                'bool
'                aFieldDesc.Datatype = otFieldDataType.Bool
'                aFieldDesc.Title = "value as bool"
'                aFieldDesc.ColumnName = "valuebool"
'                aFieldDesc.ID = "cfi7"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)


'                'bool
'                aFieldDesc.Datatype = otFieldDataType.[Long]
'                aFieldDesc.Title = "value as long"
'                aFieldDesc.ColumnName = "valuelong"
'                aFieldDesc.ID = "cfi8"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                'datatype
'                aFieldDesc.Datatype = otFieldDataType.[Long]
'                aFieldDesc.Title = "datatype"
'                aFieldDesc.ColumnName = "datatype"
'                aFieldDesc.ID = "cfi10"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                ' cmt
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "message log tag"
'                aFieldDesc.ColumnName = "msglogtag"
'                aFieldDesc.ID = "cfi13"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)


'                ' msglogtag
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "comment"
'                aFieldDesc.ColumnName = "cmt"
'                aFieldDesc.ID = "cfi14"
'                aFieldDesc.Size = 100
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                ' parameter_txt 1
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "parameter_txt 1"
'                aFieldDesc.ColumnName = "param_txt1"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                ' parameter_txt 2
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "parameter_txt 2 of condition"
'                aFieldDesc.ColumnName = "param_txt2"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                ' parameter_txt 2
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "parameter_txt 3 of condition"
'                aFieldDesc.ColumnName = "param_txt3"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                ' parameter_num 1
'                aFieldDesc.Datatype = otFieldDataType.Numeric
'                aFieldDesc.Title = "parameter numeric 1 of condition"
'                aFieldDesc.ColumnName = "param_num1"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                ' parameter_num 2
'                aFieldDesc.Datatype = otFieldDataType.Numeric
'                aFieldDesc.Title = "parameter numeric 2 of condition"
'                aFieldDesc.ColumnName = "param_num2"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                ' parameter_num 2
'                aFieldDesc.Datatype = otFieldDataType.Numeric
'                aFieldDesc.Title = "parameter numeric 3 of condition"
'                aFieldDesc.ColumnName = "param_num3"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                ' parameter_date 1
'                aFieldDesc.Datatype = otFieldDataType.[Date]
'                aFieldDesc.Title = "parameter date 1 of condition"
'                aFieldDesc.ColumnName = "param_date1"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                ' parameter_date 2
'                aFieldDesc.Datatype = otFieldDataType.[Date]
'                aFieldDesc.Title = "parameter date 2 of condition"
'                aFieldDesc.ColumnName = "param_date2"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                ' parameter_date 3
'                aFieldDesc.Datatype = otFieldDataType.[Date]
'                aFieldDesc.Title = "parameter date 3 of condition"
'                aFieldDesc.ColumnName = "param_date3"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                ' parameter_flag 1
'                aFieldDesc.Datatype = otFieldDataType.Bool
'                aFieldDesc.Title = "parameter flag 1 of condition"
'                aFieldDesc.ColumnName = "param_flag1"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                ' parameter_flag 2
'                aFieldDesc.Datatype = otFieldDataType.Bool
'                aFieldDesc.Title = "parameter flag 2 of condition"
'                aFieldDesc.ColumnName = "param_flag2"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                ' parameter_flag 3
'                aFieldDesc.Datatype = otFieldDataType.Bool
'                aFieldDesc.Title = "parameter flag 3 of condition"
'                aFieldDesc.ColumnName = "param_flag3"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                '***
'                '*** TIMESTAMP
'                '****
'                aFieldDesc.Datatype = otFieldDataType.Timestamp
'                aFieldDesc.Title = "last Update"
'                aFieldDesc.ColumnName = ConstFNUpdatedOn
'                aFieldDesc.ID = ""
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                aFieldDesc.Datatype = otFieldDataType.Timestamp
'                aFieldDesc.Title = "creation Date"
'                aFieldDesc.ColumnName = ConstFNCreatedOn
'                aFieldDesc.ID = ""
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                ' Index
'                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
'                Call .AddIndex(ConstDefaultCompoundIndexName, CompoundIndexColumnNames, isprimarykey:=False)
'                ' persist
'                .Persist()
'                ' change the database
'                .CreateObjectSchema()
'            End With

'            ' reset the Table description
'            If Not Me.Record.SetTable(ourTableName, forceReload:=True) Then
'                Call CoreMessageHandler(subname:="clsOTDBSchedule.createSchema", tablename:=ourTableName, _
'                                      message:="Error while setTable in createSchema")
'            End If

'            createSchema = True
'            Exit Function

'            ' Handle the error
'error_handle:
'            Call CoreMessageHandler(subname:="clsOTDBConfigurableItem.createSchema")
'            createSchema = False
        End Function

        '****
        '**** updateRecord
        Private Function updateRecord() As Boolean
            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    updateRecord = False
                    Exit Function
                End If
            End If
            If Not _IsLoaded And Not Me.IsCreated And Not s_isCacheNoSave Then
                updateRecord = False
                Exit Function
            End If

            If Not Me.Record.Alive Then
                updateRecord = False
                Exit Function
            End If

            'On Error GoTo errorhandle
            Call Me.Record.SetValue("uid", s_uid)
            Call Me.Record.SetValue("cname", s_configname)
            Call Me.Record.SetValue("id", s_id)
            Call Me.Record.SetValue("cmt", s_cmt)

            Call Me.Record.SetValue("datatype", s_datatype)

            Call Me.Record.SetValue("valuedate", ConstNullDate)
            Call Me.Record.SetValue("valuenumeric", 0)
            Call Me.Record.SetValue("valuelong", 0)
            Call Me.Record.SetValue("valuebool", False)

            Select Case s_datatype

                Case otFieldDataType.Numeric
                    If IsNumeric(s_value) Then Call Me.Record.SetValue("valuenumeric", CDbl(s_value))
                    Call Me.Record.SetValue("value", CStr(s_value))
                Case otFieldDataType.List, otFieldDataType.Text, otFieldDataType.Memo
                    Call Me.Record.SetValue("value", CStr(s_value))
                Case otFieldDataType.Runtime, otFieldDataType.Formula, otFieldDataType.Binary
                    Call CoreMessageHandler(subname:="clsOTDBConfigurableItem.persist", _
                                          message:="datatype (runtime, formular, binary) not specified how to be persisted", msglog:=s_msglog, arg1:=s_datatype)
                Case otFieldDataType.[Date]
                    If IsDate(s_value) Then
                        Call Me.Record.SetValue("valuedate", CDate(s_value))
                        Call Me.Record.SetValue("value", Format(s_value, "dd.mm.yyyy"))
                    Else
                        Call Me.Record.SetValue("value", CStr(s_value))
                    End If
                Case otFieldDataType.[Long]
                    If IsNumeric(s_value) Then Call Me.Record.SetValue("valuelong", CLng(s_value))
                    Call Me.Record.SetValue("value", CStr(s_value))
                Case otFieldDataType.Timestamp
                    If IsDate(s_value) Then
                        Call Me.Record.SetValue("valuedate", CDate(s_value))
                        Call Me.Record.SetValue("value", Format(s_value, "dd.mm.yyyy hh:mm:ss"))
                    Else
                        Call Me.Record.SetValue("value", CStr(s_value))
                    End If
                Case otFieldDataType.Bool
                    If s_value = "" Or IsNothing(s_value) Or IsNull(s_value) Or s_value Is Nothing Then
                        Call Me.Record.SetValue("valuebool", False)
                    ElseIf s_value = True Or s_value = False Then
                        Call Me.Record.SetValue("valuebool", CBool(s_value))
                    Else
                        Call Me.Record.SetValue("valuebool", True)
                    End If
                    Call Me.Record.SetValue("value", CStr(s_value))
                Case Else
                    Call Me.Record.SetValue("value", CStr(s_value))
                    Call CoreMessageHandler(subname:="clsOTDBConfigurableItem.persist", _
                                          message:="datatype not specified how to be persisted", msglog:=s_msglog, arg1:=s_datatype)
            End Select


            Call Me.Record.SetValue("msglogtag", s_msglogtag)

            Call Me.Record.SetValue("param_txt1", s_parameter_txt1)
            Call Me.Record.SetValue("param_txt2", s_parameter_txt2)
            Call Me.Record.SetValue("param_txt3", s_parameter_txt3)
            Call Me.Record.SetValue("param_date1", s_parameter_date1)
            Call Me.Record.SetValue("param_date2", s_parameter_date2)
            Call Me.Record.SetValue("param_date3", s_parameter_date3)
            Call Me.Record.SetValue("param_num1", s_parameter_num1)
            Call Me.Record.SetValue("param_num2", s_parameter_num2)
            Call Me.Record.SetValue("param_num3", s_parameter_num3)
            Call Me.Record.SetValue("param_flag1", s_parameter_flag1)
            Call Me.Record.SetValue("param_flag2", s_parameter_flag2)
            Call Me.Record.SetValue("param_flag3", s_parameter_flag3)

            updateRecord = True
        End Function

        '**** persist
        '****

        Public Function PERSIST(Optional TIMESTAMP As Date = Nothing, _
                            Optional ForceSerializeToOTDB As Boolean = False) As Boolean

            ' update the Record first
            If Not updateRecord() Then
                PERSIST = False
                Exit Function
            End If

            '*** overload it from the Application Container
            '***
            'If Me.serializeWithHostApplication Then
            '    If overwriteToHostApplication(me.record) Then
            '        s_savedToHost = True
            '    End If
            'End If
            'If ForceSerializeToOTDB Or Not Me.serializeWithHostApplication Then
            ' persist with update timestamp
            If IsMissing(TIMESTAMP) Or Not IsDate(TIMESTAMP) Then
                PERSIST = Me.Record.Persist
            Else
                PERSIST = Me.Record.Persist(TIMESTAMP)
            End If
            Exit Function
            'End If
errorhandle:

            PERSIST = False

        End Function

        '**** create : create a new Object with primary keys
        '****
        Public Function create(ByVal UID As Long, ByVal CONFIGNAME As String, ByVal ID As String, Optional ByVal FORCE As Boolean = False) As Boolean
            Dim aTable As iormDataStore
            Dim pkarry(3) As Object
            Dim aRecord As ormRecord

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    create = False
                    Exit Function
                End If
            End If

            If IsLoaded Then
                create = False
                Exit Function
            End If

            ' Check
            If Not FORCE Then
                ' set the primaryKey
                pkarry(0) = UID
                pkarry(1) = LCase(CONFIGNAME)
                pkarry(2) = LCase(ID)
                'PKArry(3) = dependfrompartid
                aTable = GetTableStore(ourTableName)
                aRecord = aTable.GetRecordByPrimaryKey(pkarry)

                If Not aRecord Is Nothing Then
                    create = False
                    'Call OTDBErrorHandler(tablename:=ourTableName, entryname:="partid, posno", _
                    'subname:="clsOTDBBOMMember.create", message:=" double key as should be unique", arg1:=partid & posno)
                    Exit Function
                End If
            End If
            ' set the primaryKey
            s_uid = UID
            s_configname = LCase(CONFIGNAME)
            s_id = LCase(ID)

            _IsCreated = True
            create = Me.IsCreated

        End Function

        '**** clone the object
        '****
        Public Function clone(ByVal UID As Long, ByVal CONFIGNAME As String, ByVal ID As String) As clsOTDBConfigurableItem
            '*** now we copy the object
            Dim aNewObject As New clsOTDBConfigurableItem
            Dim newRecord As New ormRecord

            Dim m As Object
            Dim aVAlue As Object

            If Not IsLoaded And Not IsCreated Then
                clone = Nothing
                Exit Function
            End If
            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    clone = Nothing
                    Exit Function
                End If
            End If

            'update our Record
            If Not updateRecord() Then
                clone = Nothing
                Exit Function
            End If

            If Not aNewObject.create(UID:=UID, CONFIGNAME:=CONFIGNAME, ID:=ID, FORCE:=True) Then
                clone = Nothing
                Exit Function
            End If

            ' set it
            newRecord.SetTable(Me.TableID)

            ' go through the table and overwrite the Record if the rights are there
            For Each m In Me.Record.keys
                If m <> ConstFNCreatedOn And m <> ConstFNUpdatedOn Then
                    Call newRecord.SetValue(m, Me.Record.GetValue(m))
                End If
            Next m

            ' overwrite the primary keys
            Call newRecord.SetValue("uid", UID)
            Call newRecord.SetValue("cname", LCase(CONFIGNAME))
            Call newRecord.SetValue("id", LCase(ID))


            If aNewObject.infuse(newRecord) Then
                clone = aNewObject
            Else
                clone = Nothing
            End If
        End Function


    End Class


    Public Class clsOTDBConfigurableLink

        Inherits ormDataObject

        '************************************************************************************
        '***** CLASS clsOTDBConfigurableLink is the object for a OTDBRecord (which is the datastore)
        '*****
        '*****
        Const ourTableName = "tblConfigLinks"

        Private s_uid As Long
        Private s_configname As String
        Private s_objectname As String
        Private s_tag As Object
        Private s_isActive As Boolean
        Private s_datatype As otFieldDataType


        '** initialize
        Public Sub New()
            MyBase.New(ourTableName)
            'me.record.tablename = ourTableName
        End Sub
        '*** init
        Public Function initialize() As Boolean
            initialize = MyBase.Initialize

        End Function


        ReadOnly Property UID() As Long
            Get
                UID = s_uid
            End Get

        End Property


        Public Property CONFIGNAME() As String
            Get
                CONFIGNAME = s_configname
            End Get
            Set(value As String)
                s_configname = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property OBJECTNAME() As String
            Get
                OBJECTNAME = s_objectname
            End Get
            Set(value As String)
                s_objectname = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property TAG() As Object
            Get
                TAG = s_tag
            End Get
            Set(value As Object)
                s_tag = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property DATATYPE() As otFieldDataType
            Get
                DATATYPE = s_datatype
            End Get
            Set(value As otFieldDataType)
                If s_datatype <> value Then
                    s_datatype = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property isActive() As Boolean
            Get
                isActive = s_isActive
            End Get
            Set(value As Boolean)
                s_isActive = value
                Me.IsChanged = True
            End Set
        End Property

        '****** allByUID: "static" function to return a collection of curSchedules by key
        '******
        Public Function allByUID(UID As Long) As Collection
            Dim aCollection As New Collection
            Dim aRECORDCollection As List(Of ormRecord)
            Dim aTable As iormDataStore
            Dim Key() As Object
            Dim RECORD As ormRecord
            Dim aNewLink As New clsOTDBConfigurableLink
            ' set the primaryKey
            ReDim Key(1)

            Key(0) = UID

            On Error GoTo error_handler

            aTable = GetTableStore(ourTableName)
            aRECORDCollection = aTable.GetRecordsBySql(wherestr:=" uid = " & CStr(UID))

            If aRECORDCollection Is Nothing Then
                Me.Unload()
                allByUID = Nothing
                Exit Function
            Else
                For Each RECORD In aRECORDCollection
                    aNewLink = New clsOTDBConfigurableLink
                    If aNewLink.Infuse(RECORD) Then
                        aCollection.Add(Item:=aNewLink)
                    End If
                Next RECORD
                allByUID = aCollection
                Exit Function
            End If

error_handler:

            allByUID = Nothing
            Exit Function
        End Function

        '****** allByconfigname: "static" function to return a collection of curSchedules by key
        '******
        Public Function allByconfigname(ByVal CONFIGNAME As String) As Collection
            Dim aCollection As New Collection
            Dim aRECORDCollection As List(Of ormRecord)
            Dim aTable As iormDataStore
            Dim Key() As Object
            Dim aRecord As ormRecord
            Dim aNewLink As New clsOTDBConfigurableLink
            Dim orderby As String

            ' set the primaryKey
            ReDim Key(1)
            Key(0) = UID

            On Error GoTo error_handler
            orderby = "uid asc"

            aTable = GetTableStore(ourTableName)
            aRECORDCollection = aTable.GetRecordsBySql(wherestr:=" cname ='" & CONFIGNAME & "'", orderby:=orderby)

            If aRECORDCollection Is Nothing Then
                Me.Unload()
                allByconfigname = Nothing
                Exit Function
            Else
                For Each aRecord In aRECORDCollection
                    aNewLink = New clsOTDBConfigurableLink
                    If aNewLink.Infuse(aRecord) Then
                        aCollection.Add(Item:=aNewLink)
                    End If
                Next aRecord
                allByconfigname = aCollection
                Exit Function
            End If

error_handler:

            allByconfigname = Nothing
            Exit Function
        End Function

        '****** allByUIDConfig: "static" function to return a collection
        '******
        Public Function allByUIDConfig(ByVal UID As Long, ByVal CONFIGNAME As String) As Collection
            Dim aCollection As New Collection
            Dim aRECORDCollection As List(Of ormRecord)
            Dim aTable As iormDataStore
            Dim Key() As Object
            Dim aRECORD As ormRecord
            Dim aNewLink As New clsOTDBConfigurableLink

            ' set the primaryKey
            ReDim Key(2)
            Key(0) = UID
            Key(1) = LCase(CONFIGNAME)

            On Error GoTo error_handler

            aTable = GetTableStore(ourTableName)
            aRECORDCollection = aTable.GetRecordsBySql(wherestr:=" uid = " & CStr(UID) & _
                                                                    " and cname = '" & LCase(CONFIGNAME) & "'")
            If aRECORDCollection Is Nothing Then
                Me.Unload()
                allByUIDConfig = Nothing
                Exit Function
            Else
                For Each aRECORD In aRECORDCollection
                    aNewLink = New clsOTDBConfigurableLink

                    If aNewLink.Infuse(aRECORD) Then
                        aCollection.Add(Item:=aNewLink)
                    End If
                Next
                allByUIDConfig = aCollection
                Exit Function
            End If

error_handler:

            allByUIDConfig = Nothing
            Exit Function
        End Function

        '********** static createSchema
        '**********
        Public Function createSchema(Optional ByVal silent As Boolean = True) As Boolean


'            Dim aFieldDesc As New ormFieldDescription
'            Dim PrimaryColumnNames As New Collection
'            Dim aTable As New ObjectDefinition

'            With aTable
'                .Create(ourTableName)
'                .Delete()

'                aFieldDesc.Tablename = ourTableName
'                aFieldDesc.ID = ""
'                aFieldDesc.Parameter = ""

'                '*** UID
'                '**** UID
'                aFieldDesc.Datatype = otFieldDataType.[Long]
'                aFieldDesc.Title = "uid of config"
'                aFieldDesc.Aliases = New String() {}
'                aFieldDesc.ID = "cnfl2"
'                aFieldDesc.ColumnName = "uid"
'                aFieldDesc.Size = 0
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "configname"
'                aFieldDesc.ID = "cnfl2"
'                aFieldDesc.ColumnName = "cname"
'                aFieldDesc.Size = 50
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                PrimaryColumnNames.Add(aFieldDesc.ColumnName)




'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "objectname to link"
'                aFieldDesc.ID = "cnfl3"
'                aFieldDesc.Aliases = New String() {}
'                aFieldDesc.ColumnName = "objectname"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                '**** configtag
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "tag of objectname"
'                aFieldDesc.ID = "cnfl4"
'                aFieldDesc.Aliases = New String() {}
'                aFieldDesc.ColumnName = "tag"
'                aFieldDesc.Size = 100
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

'                '***** isactive
'                aFieldDesc.Datatype = otFieldDataType.Bool
'                aFieldDesc.Title = "is an active setting"
'                aFieldDesc.Aliases = New String() {}
'                aFieldDesc.ID = "cnfl5"
'                aFieldDesc.ColumnName = "isactive"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                '***** message log tag
'                aFieldDesc.Datatype = otFieldDataType.[Long]
'                aFieldDesc.Title = "datatype of tag"
'                aFieldDesc.Aliases = New String() {}
'                aFieldDesc.ID = ""
'                aFieldDesc.ColumnName = "datatype"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                '***** message log tag
'                aFieldDesc.Datatype = otFieldDataType.Text
'                aFieldDesc.Title = "message log tag"
'                aFieldDesc.Aliases = New String() {}
'                aFieldDesc.ID = ""
'                aFieldDesc.ColumnName = "msglogtag"
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                '***
'                '*** TIMESTAMP
'                '****
'                aFieldDesc.Datatype = otFieldDataType.Timestamp
'                aFieldDesc.Title = "last Update"
'                aFieldDesc.ColumnName = ConstFNUpdatedOn
'                aFieldDesc.ID = ""
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)

'                aFieldDesc.Datatype = otFieldDataType.Timestamp
'                aFieldDesc.Title = "creation Date"
'                aFieldDesc.ColumnName = ConstFNCreatedOn
'                aFieldDesc.ID = ""
'                Call .AddFieldDesc(fielddesc:=aFieldDesc)
'                ' Index
'                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)

'                ' persist
'                .Persist()
'                ' change the database
'                .CreateObjectSchema()

'            End With

'            ' reset the Table description
'            If Not Me.Record.SetTable(ourTableName, forceReload:=True) Then
'                Call CoreMessageHandler(subname:="clsOTDBConfigurableLink.createSchema", tablename:=ourTableName, _
'                                      message:="Error while setTable in createSchema")
'            End If

'            '
'            createSchema = True
'            Exit Function

'            ' Handle the error
'error_handle:
'            Call CoreMessageHandler(subname:="clsOTDBConfigurableLink.createSchema", tablename:=ourTableName)
'            createSchema = False
        End Function

        '**** infuse the the Object by a OTBRecord
        '****
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    Infuse = False
                    Exit Function
                End If
            End If


            On Error GoTo errorhandle

            Me.Record = record
            s_uid = CLng(record.GetValue("uid"))
            s_configname = CStr(record.GetValue("cname"))
            s_objectname = CStr(record.GetValue("objectname"))
            s_tag = CStr(record.GetValue("tag"))
            s_datatype = CLng(record.GetValue("datatype"))
            s_isActive = CBool(record.GetValue("isactive"))
            's_updc = CLng(RECORD.getValue("updc"))

            If IsDate(record.GetValue(ConstFNCreatedOn)) Then
                _createdOn = CDate(record.GetValue(ConstFNCreatedOn))
            Else
                _createdOn = ConstNullDate
            End If
            _updatedOn = CDate(record.GetValue(ConstFNUpdatedOn))
            _IsLoaded = True
            Infuse = True
            Exit Function

errorhandle:
            Infuse = False


        End Function


        '****** getCurrSchedule entry
        '******
        Public Function Inject(ByVal UID As Long, _
                               ByVal CONFIGNAME As String, _
                               ByVal TAG As Object) As Boolean
            Dim aTable As iormDataStore
            Dim pkarry() As Object
            Dim aRecord As ormRecord

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    Inject = False
                    Exit Function
                End If
            End If

            ' set the primaryKey
            ReDim pkarry(3)
            pkarry(0) = UID
            pkarry(1) = LCase(CONFIGNAME)
            'pkarry(3) = LCase(OBJECTNAME)
            pkarry(2) = TAG

            aTable = GetTableStore(Me.Record.TableID)
            aRecord = aTable.GetRecordByPrimaryKey(pkarry)

            If aRecord Is Nothing Then
                Me.Unload()
                Inject = Me.IsLoaded
                Exit Function
            Else
                Me.Record = aRecord
                _IsLoaded = Me.Infuse(Me.Record)
                Inject = Me.IsLoaded
                Exit Function
            End If
        End Function

        '**** persist
        '****

        Public Function PERSIST(Optional TIMESTAMP As Date = Nothing) As Boolean
            Dim aTable As iormDataStore
            Dim i As Integer
            Dim fieldname As String
            Dim aVAlue As Object

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    PERSIST = False
                    Exit Function
                End If
            End If


            On Error GoTo errorhandle

            'On Error GoTo errorhandle
            Call Me.Record.SetValue("uid", s_uid)
            Call Me.Record.SetValue("tag", s_tag)
            Call Me.Record.SetValue("cname", s_configname)
            Call Me.Record.SetValue("datatype", s_datatype)
            Call Me.Record.SetValue("isactive", s_isActive)
            Call Me.Record.SetValue("objectname", s_objectname)

            'Call me.record.setValue(OTDBConst_UpdateOn, (Date & " " & Time)) not necessary
            If IsMissing(TIMESTAMP) Or Not IsDate(TIMESTAMP) Then
                PERSIST = Me.Record.Persist
            Else
                PERSIST = Me.Record.Persist(TIMESTAMP)
            End If

            Exit Function

errorhandle:

            PERSIST = False

        End Function
        '**** create : create a new Object with primary keys
        '****
        Public Function create(ByVal UID As Long, _
                               ByVal CONFIGNAME As String, _
                               ByVal TAG As Object) As Boolean
            Dim aTable As iormDataStore
            Dim pkarry() As Object
            Dim aRecord As ormRecord

            '* init
            If Not Me.IsInitialized Then
                If Not Me.initialize() Then
                    create = False
                    Exit Function
                End If
            End If


            If IsLoaded Then
                create = False
                Exit Function
            End If

            ' Check
            ' set the primaryKey
            ReDim pkarry(3)
            pkarry(0) = UID
            pkarry(1) = LCase(CONFIGNAME)
            'pkarry(3) = LCase(OBJECTNAME)
            pkarry(2) = TAG
            'PKArry(3) = dependfrompartid
            aTable = GetTableStore(ourTableName)
            aRecord = aTable.GetRecordByPrimaryKey(pkarry)

            If Not aRecord Is Nothing Then
                create = False
                'Call OTDBErrorHandler(tablename:=ourTableName, entryname:="partid, posno", _
                'subname:="clsOTDBBOMMember.create", message:=" double key as should be unique", arg1:=partid & posno)
                Exit Function
            End If

            ' set the primaryKey
            s_uid = UID
            s_configname = LCase(CONFIGNAME)
            's_objectname = LCase(OBJECTNAME)
            s_tag = TAG
            s_isActive = True

            _IsCreated = True
            create = Me.IsCreated

        End Function

    End Class
End Namespace
