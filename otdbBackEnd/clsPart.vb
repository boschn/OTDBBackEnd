

REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** BUSINESS OBJECTs CLASSES: Parts
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2013-08-08
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************


Option Explicit On
Imports System.Data
Imports System.Data.OleDb
Imports System.Collections.Generic
Imports System.IO
Imports System.Diagnostics.Debug

Imports OnTrack.Database
Imports OnTrack
Imports OnTrack.Deliverables

Namespace OnTrack.Parts


    '************************************************************************************
    '***** CLASS Part is the object for a OTDBRecord (which is the datastore)
    '*****
    '*****
    <ormObject(id:=Part.ConstObjectID, modulename:=ConstModuleParts, Version:=1)> _
    Public Class Part
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "Part"

        '*** SCHEMA TABLE
        <ormSchemaTable(Version:=2, AdddeleteFieldBehavior:=True, addsparefields:=True)> Public Const ConstTableID As String = "tblParts"

        '*** Primary key
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, primarykeyOrdinal:=1, _
            XID:="pt1", Aliases:={"C10"}, title:="PartID", description:="unique ID of the part")> Public Const ConstFNPartID = "pnid"

        '** Indices
        <ormSchemaIndex(columnname1:=ConstFNIsDeleted, columnname2:=ConstFNPartID)> Public Const ConstIndexDeleted = "indDeleted"
        <ormSchemaIndex(columnname1:=constFNMatchCode, columnname2:=ConstFNPartID, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexMatchcode = "indmatchcode"
        <ormSchemaIndex(columnname1:=constFNCategory, columnname2:=ConstFNPartID, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexcategory = "indcategory"
        <ormSchemaIndex(columnname1:=constFNFunction, columnname2:=ConstFNPartID, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexFunction = "indFunction"
        <ormSchemaIndex(columnname1:=constFNTypeID, columnname2:=ConstFNPartID, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexType = "indType"
        <ormSchemaIndex(columnName1:=ConstFNDomainID, columnname2:=ConstFNPartID, columnname3:=ConstFNIsDeleted)> Public Const constIndexDomain = "indDomains"

        '*** Fields
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID)> Public Const ConstFNDomainID = Domain.ConstFNDomainID
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=150, _
            XID:="pt2", Title:="Description", description:="description of the part")> Public Const ConstFNDescription = "desc"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=150, _
           XID:="pt3", aliases:={"DLV31"}, Title:="Workpackage", description:="workpackage of the part")> Public Const ConstFNWorkpackage = "wkpk"
        <ormObjectEntry(referenceobjectentry:=OnTrack.Workspace.ConstObjectID & "." & OnTrack.Workspace.ConstFNID, _
           Description:="workspaceID ID of the part")> Public Const ConstFNWorkspace = OnTrack.Workspace.ConstFNID
        <ormObjectEntry(referenceobjectentry:=Deliverables.Deliverable.ConstObjectID & "." & Deliverables.Deliverable.constFNUid, isnullable:=True, _
           XID:="DLV1", aliases:={"UID"}, Description:="deliverable UID of the part")> Public Const ConstFNDeliverableUID = Deliverables.Deliverable.constFNUid
        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, _
            XID:="pt4", Title:="Responsible", description:="responsible person for the deliverable", XID:="DLV16")> Public Const constFNResponsiblePerson = "resp"
        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, _
            XID:="pt5", title:="Responsible OrgUnit", description:=" organization unit responsible for the part", XID:="")> Public Const constFNRespOU = "respou"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, _
            XID:="pt6", title:="Type", description:="type of the part", XID:="DLV13")> Public Const constFNTypeID = "typeid"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=150, _
            XID:="pt7", title:="Category", description:="category of the part", XID:="DLV13")> Public Const constFNCategory = "cat"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, defaultValue:="", _
            XID:="pt8", title:="blocking item reference", description:="blocking item reference id for the deliverable", aliases:={"DLV17"})> Public Const constFNBlockingItemReference = "blitemid"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, defaultValue:="", _
            XID:="pt9", aliases:={"dlv8"}, title:="Change Reference", description:="change reference of the deliverable")> Public Const constFNChangeRef = "chref"
        <ormObjectEntry(typeid:=otFieldDataType.Memo, defaultValue:="", _
            XID:="pt10", title:="comment", description:="comments of the part", XID:="DLV18")> Public Const constFNComment = "cmt"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, defaultValue:="", _
            XID:="pt11", title:="Matchcode", description:="match code of the part")> Public Const constFNMatchCode = "matchcode"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, defaultValue:="", _
             XID:="pt12", Title:="Function", description:="function of the deliverable")> Public Const constFNFunction = "function"

        <ormObjectEntry(referenceObjectEntry:=ObjectLogMessage.ConstObjectID & "." & ObjectLogMessage.ConstFNTag)> Public Const ConstFNmsglogtag = ObjectLogMessage.ConstFNTag
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=255, defaultValue:="", _
            title:="ConfigTag", description:="config tag for the part")> Public Const constFNConfigTag = "cnftag"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=255, defaultValue:="", _
            title:="ActivityTag", description:="activity tag for the part")> Public Const constFNActiveTag = "acttag"

        '*** Mappings
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String
        <ormEntryMapping(EntryName:=ConstFNDeliverableUID)> Private _deliverableUID As Long
        <ormEntryMapping(EntryName:=ConstFNPartID)> Private _partID As String    ' unique key
        <ormEntryMapping(EntryName:=constFNFunction)> Private _Function As String
        <ormEntryMapping(EntryName:=constFNTypeID)> Private _typeid As String
        <ormEntryMapping(EntryName:=ConstFNWorkspace)> Private _workspaceID As String
        <ormEntryMapping(EntryName:=constFNRespOU)> Private _respOU As String
        <ormEntryMapping(EntryName:=ConstFNWorkpackage)> Private _workpackage As String
        <ormEntryMapping(EntryName:=constFNResponsiblePerson)> Private _responsible As String
        <ormEntryMapping(EntryName:=constFNChangeRef)> Private _changerefID As String
        <ormEntryMapping(EntryName:=constFNComment)> Private _comment As String
        <ormEntryMapping(EntryName:=ConstFNmsglogtag)> Private _msglogtag As String
        <ormEntryMapping(EntryName:=constFNBlockingItemReference)> Private _blockingitemID As String
        <ormEntryMapping(EntryName:=constFNCategory)> Private _cat As String
        <ormEntryMapping(EntryName:=constFNMatchCode)> Private _matchcode As String
        <ormEntryMapping(EntryName:=constFNConfigTag)> Private _configtag As String
        <ormEntryMapping(EntryName:=constFNActiveTag)> Private _activetag As String
        ' dynamic
        Private s_interfaceCollection As New Collection


        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub


#Region "properties"
        ''' <summary>
        ''' gets the unique PARTID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property PartID() As String
            Get
                PartID = _partID
            End Get

        End Property
        ''' <summary>
        ''' sets or gets the linkes Deliverable UID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DeliverableUID() As Long
            Get
                DeliverableUID = _deliverableUID
            End Get
            Set(value As Long)
                If _deliverableUID <> value Then
                    _deliverableUID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the workpackage code
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Workpackage() As String
            Get
                Workpackage = _workpackage
            End Get
            Set(value As String)
                If _workpackage <> value Then
                    _workpackage = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the Workspace
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Workspace() As String
            Get
                Workspace = _workspaceID
            End Get
            Set(value As String)
                If _workspaceID <> value Then
                    _workspaceID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the Description
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                If value <> _description Then
                    _description = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the category
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Category() As String
            Get
                Category = _cat
            End Get
            Set(value As String)
                If value <> _cat Then
                    _cat = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the function.
        ''' </summary>
        ''' <value>The function.</value>
        Public Property [Function]() As String
            Get
                Return Me._Function
            End Get
            Set(value As String)
                Me._Function = value
            End Set
        End Property

        ''' <summary>
        ''' sets or gets the responsible Person for the Part
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Responsible() As String
            Get
                Responsible = _responsible
            End Get
            Set(value As String)
                If value <> _responsible Then
                    _responsible = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' sets or gets the Responsible OU
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ResponsibleOU() As String
            Get
                ResponsibleOU = _respOU
            End Get
            Set(value As String)
                If value <> _respOU Then
                    _respOU = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' Sets or gets the BlockingItem Reference
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property BlockingItemID() As String
            Get
                BlockingItemID = _blockingitemID
            End Get
            Set(value As String)
                If _blockingitemID <> value Then
                    _blockingitemID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the Part-Type
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Property Parttype() As String
            Get
                Parttype = _typeid
            End Get
            Set(value As String)
                If _typeid <> value Then
                    _typeid = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the MatchCode
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Property Matchcode() As String
            Get
                Matchcode = _matchcode
            End Get
            Set(value As String)
                If _matchcode <> value Then
                    _matchcode = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or set the ChangeReferenceID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ChangeReferenceID() As String
            Get
                ChangeReferenceID = _changerefID
            End Get
            Set(value As String)
                If _changerefID <> value Then
                    _changerefID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the general Comment
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Comment() As String
            Get
                Comment = _comment
            End Get
            Set(value As String)
                _comment = value
                Me.IsChanged = True
            End Set
        End Property

        '****** createTAG
        Public Function getUniqueTag()
            getUniqueTag = ConstDelimiter & ConstTableID & ConstDelimiter & _partID & ConstDelimiter
        End Function
        ReadOnly Property Msglogtag() As String
            Get
                If _msglogtag = "" Then
                    _msglogtag = getUniqueTag()
                End If
                Msglogtag = _msglogtag
            End Get

        End Property

        ReadOnly Property Configtag()
            Get
                If _configtag = "" Then
                    _configtag = getUniqueTag()
                End If
                Configtag = _configtag
            End Get
        End Property
#End Region

        ''' <summary>
        ''' return all Parts as List
        ''' </summary>
        ''' <param name="isDeleted"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function All(Optional isDeleted As Boolean = False) As List(Of Part)
            Return ormDataObject.AllDataObject(Of Part)(deleted:=isDeleted)
        End Function

        ''' <summary>
        ''' return a List of parts by deliverableUID
        ''' </summary>
        ''' <param name="deliverableUID"></param>
        ''' <param name="isDeleted"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AllByDeliverable(ByVal deliverableUID As Long, Optional ByVal isDeleted As Boolean = False) As List(Of Part)
            Return ormDataObject.AllDataObject(Of Part)(deleted:=isDeleted, where:="[" & ConstFNDeliverableUID & "] = @dlvuid", _
                                              parameters:={New ormSqlCommandParameter(ID:="@dlvuid", ColumnName:=ConstFNDeliverableUID, value:=deliverableUID, tablename:=ConstTableID)}.ToList)

        End Function

        '****** all: "static" function to return a collection of parts by key
        '******
        Public Function AllByPrecodeAndOU(ByVal precode As String, _
                                          Optional ByVal department As String = "", _
                                          Optional ByVal site As String = "", _
                                          Optional ByVal workpackage As String = "") As Collection
            Dim aCollection As New Collection
            Dim aRecordCollection As List(Of ormRecord)
            Dim aTable As iormDataStore
            Dim Key() As Object
            Dim aRecord As ormRecord
            Dim wherestr As String
            Dim innerjoin As String
            Dim textstr As String

            ' set the primaryKey
            ReDim Key(0)
            Key(0) = DeliverableUID

            On Error GoTo error_handler

            aTable = GetTableStore(ConstTableID)
            ' get rid of the '.'
            'precode = RemoveChar(precode, ".")
            textstr = Mid(precode, 1, 1) & Mid(precode, 3, 3)

            wherestr = "mid(" & ConstTableID & ".pnid,1,4) ='" & textstr & "' "
            ' select
            If department <> "" Then
                wherestr = wherestr & " and " & ConstTableID & ".dept ='" & department & "' "
            End If
            If site <> "" Then
                wherestr = wherestr & " and " & ConstTableID & ".site ='" & site & "' "
            End If
            If workpackage <> "" Then
                wherestr = wherestr & " and " & ConstTableID & ".wkpk ='" & workpackage & "' "
            End If

            aRecordCollection = aTable.GetRecordsBySql(wherestr:=wherestr)

            If aRecordCollection Is Nothing Then
                Me.Unload()
                AllByPrecodeAndOU = Nothing
                Exit Function
            Else
                For Each aRecord In aRecordCollection
                    Dim aNewPart As New Part
                    If InfuseDataObject(record:=aRecord, dataobject:=aNewPart) Then
                        aCollection.Add(Item:=aNewPart)
                    End If
                Next aRecord
                AllByPrecodeAndOU = aCollection
                Exit Function
            End If

error_handler:

            AllByPrecodeAndOU = Nothing
            Exit Function
        End Function


        ''' <summary>
        ''' Load by Primary Key
        ''' </summary>
        ''' <param name="pnid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Inject(pnid As String) As Boolean
            Dim primarykey() As Object = {pnid}
            Return MyBase.Inject(primarykey)
        End Function

        ''' <summary>
        ''' create persistency Schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of Part)(silent:=silent)
            '            Dim aFieldDesc As New ormFieldDescription
            '            Dim PrimaryColumnNames As New Collection
            '            Dim aTable As New ObjectDefinition


            '            aFieldDesc.ID = ""
            '            aFieldDesc.Parameter = ""
            '            aFieldDesc.Relation = New String() {}
            '            aFieldDesc.Aliases = New String() {}
            '            aFieldDesc.Tablename = constTableID

            '            With aTable
            '                .Create(constTableID)
            '                .Delete()

            '                '***
            '                '*** Fields
            '                '****

            '                'Type
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "partid"
            '                aFieldDesc.ColumnName = constFNPartID
            '                aFieldDesc.Aliases = New String() {"c10"}
            '                aFieldDesc.ID = "pt1"
            '                aFieldDesc.Size = 50
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "category"
            '                aFieldDesc.ColumnName = "cat"
            '                aFieldDesc.ID = "pt2"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.Size = 50
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "site"
            '                aFieldDesc.ColumnName = constFNSite
            '                aFieldDesc.Aliases = New String() {"c7"}
            '                aFieldDesc.ID = "pt3"
            '                aFieldDesc.Size = 50
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "department"
            '                aFieldDesc.ColumnName = constFNdept
            '                aFieldDesc.ID = "pt4"
            '                aFieldDesc.Aliases = New String() {"c8"}
            '                aFieldDesc.Size = 50
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "workpackage delegated site"
            '                aFieldDesc.ColumnName = constFNWkPk
            '                aFieldDesc.ID = "pt5"
            '                aFieldDesc.Aliases = New String() {"c9"}
            '                aFieldDesc.Size = 50
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "type of part"
            '                aFieldDesc.ColumnName = "typeid"
            '                aFieldDesc.ID = "pt6"
            '                aFieldDesc.Aliases = New String() {"c11"}
            '                aFieldDesc.Size = 50
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "design assurance area"
            '                aFieldDesc.ColumnName = "daar"
            '                aFieldDesc.ID = "pt7"
            '                aFieldDesc.Aliases = New String() {"c13"}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "change reference tag"
            '                aFieldDesc.ColumnName = "chref"
            '                aFieldDesc.ID = "pt8"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.Size = 50
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "responsible"
            '                aFieldDesc.ColumnName = "resp"
            '                aFieldDesc.ID = "pt9"
            '                aFieldDesc.Aliases = New String() {"c14"}
            '                aFieldDesc.Size = 100
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "description"
            '                aFieldDesc.ColumnName = "desc"
            '                aFieldDesc.ID = "pt10"
            '                aFieldDesc.Aliases = New String() {"c6"}
            '                aFieldDesc.Size = 0
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "responsible OU name"
            '                aFieldDesc.ColumnName = "respou"
            '                aFieldDesc.ID = "pt11"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.Size = 50
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "last change date"
            '                aFieldDesc.ColumnName = "chg"
            '                aFieldDesc.ID = "pt12"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.Size = 0
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "blocking item"
            '                aFieldDesc.ColumnName = "blitemid"
            '                aFieldDesc.ID = "pt17"
            '                aFieldDesc.Aliases = New String() {"bs5"}
            '                aFieldDesc.Size = 50
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' dlvUID
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "deliverable UID"
            '                aFieldDesc.ColumnName = "dlvuid"
            '                aFieldDesc.ID = "pt20"
            '                aFieldDesc.Aliases = New String() {"uid"}
            '                aFieldDesc.Size = 0
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' cmt
            '                aFieldDesc.Datatype = otFieldDataType.Memo
            '                aFieldDesc.Title = "comments"
            '                aFieldDesc.ColumnName = "cmt"
            '                aFieldDesc.ID = "pt18"
            '                aFieldDesc.Size = 0
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** configtag
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "tag of config"
            '                aFieldDesc.ID = "pt19"
            '                aFieldDesc.Aliases = New String() {"cnfl4"}
            '                aFieldDesc.Size = 100
            '                aFieldDesc.ColumnName = "cnftag"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' msglogtag
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "message log tag"
            '                aFieldDesc.ColumnName = "msglogtag"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' parameter_txt 1
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "parameter_txt 1 of condition"
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
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.Relation = New String() {}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "creation Date"
            '                aFieldDesc.ColumnName = ConstFNCreatedOn
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.Relation = New String() {}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "deletion Date"
            '                aFieldDesc.ColumnName = ConstFNDeletedOn
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.Relation = New String() {}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' parameter_flag 1
            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "Deleted"
            '                aFieldDesc.Description = "flag if field is deleted"
            '                aFieldDesc.ColumnName = ConstFNIsDeleted
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' Index
            '                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
            '                ' persist
            '                .Persist()
            '                ' change the database
            '                .AlterSchema()
            '            End With

            '            CreateSchema = True
            '            Exit Function

            '            '* reload the tablestore
            '            If CurrentSession.IsRunning Then
            '                CurrentSession.CurrentDBDriver.GetTableStore(tableID:=constTableID, force:=True)
            '            End If
            '            ' Handle the error
            'error_handle:
            '            Call CoreMessageHandler(subname:="clsOTDBdlviverable.createSchema")
            '            CreateSchema = False
        End Function

        ''' <summary>
        ''' Create an Object in the datastore
        ''' </summary>
        ''' <param name="partid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal partid As String, Optional domainid As String = "", Optional workspaceID As String = "") As Boolean
            Dim primarykey() As Object = {partid}
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID

            If MyBase.Create(primarykey, checkUnique:=True) Then
                ' set the primaryKey
                _partID = partid
                _domainID = domainid
                _workspaceID = workspaceID
                Return Me.IsCreated
            Else
                Return False
            End If

        End Function

        '****** add2InterfaceCollection adds an Interface to the InterfaceCollection of this part
        '******
        Private Function add2InterfaceCollection(ByRef anInterface As IFM.clsOTDBInterface) As Boolean
            Dim aLookupInterface As IFM.clsOTDBInterface

            ' check if we have that interface
            For Each aLookupInterface In s_interfaceCollection
                If anInterface.UID = aLookupInterface.UID Then
                    add2InterfaceCollection = False
                    Exit Function
                End If
            Next aLookupInterface

            ' add it
            s_interfaceCollection.Add(anInterface)
            add2InterfaceCollection = True

        End Function
        '****** getDocument return the Document
        '******
        Public Function GetDeliverable() As Deliverable
            Dim aDeliverable As New Deliverable

            'If _IsLoaded Then
            '    Set getDeliverable = New clsOTDBDeliverable
            '    If Not getDeliverable.Inject(Me.partid) Then
            '        Set getDeliverable = Nothing
            '    End If
            '    Exit Function
            'Else
            '    Set getDeliverable = Nothing
            '    Exit Function
            'End If

            Dim aCollection As List(Of Deliverable)
            Dim aDocument As Deliverable

            If _IsLoaded Then
                ' get the Table from the Factory
                aCollection = aDeliverable.AllByPnid(Me.PartID)
                If Not aCollection Is Nothing And aCollection.Count > 0 Then
                    GetDeliverable = aCollection.Item(0)
                    Exit Function
                End If
            End If

            GetDeliverable = Nothing
            Exit Function

        End Function

        '****** getAssyCode returns the Assycode in the partid
        '******
        Public Function GetAssycode() As String
            Dim assycode As String
            Dim substrings() As String

            On Error GoTo error_handler
            If _IsLoaded Then
                substrings = Split(Me.PartID, "-")
                If UBound(substrings) < 3 And UBound(substrings) > 0 Then
                    assycode = Mid(substrings(1), 1, 2) & "." & Mid(substrings(1), 3, 2) & "." & Mid(substrings(1), 5, 2)
                    GetAssycode = assycode
                    Exit Function
                End If
            End If

error_handler:
            GetAssycode = ""
            Exit Function
        End Function

        '****** getinterfacingParts returns the Parts to this part has interfaces with
        '******
        Public Function getInterfacingParts(Optional Sender As Boolean = True, Optional Receiver As Boolean = True) As Collection
            Dim aColInterfaces As New Collection
            Dim anInterface As IFM.clsOTDBInterface
            Dim aCartypes As clsCartypes
            Dim ourAssyCode As String
            Dim otherAssycode As String
            Dim otherPartCollection As Collection
            Dim otherPart As Part
            Dim InterfacingParts As New Collection
            Dim aDir As New Dictionary(Of String, Object)
            Dim flag As Boolean

            ''' rework
            Throw New NotImplementedException()


            If _IsLoaded Then

                ourAssyCode = Me.GetAssycode()
                'get the interfaces
                aColInterfaces = Me.GetInterfaces()
                If aColInterfaces Is Nothing Then
                    getInterfacingParts = Nothing
                    Exit Function
                End If
                aCartypes = Me.GetCartypes
                ' go through all interfaces and get the parts
                For Each anInterface In aColInterfaces
                    flag = True    ' to cointue
                    If anInterface.assy1 <> ourAssyCode Then
                        otherAssycode = anInterface.assy1
                        ' exit if we donot need senders
                        If anInterface.getAssyisSender(1) <> Sender Then
                            flag = False
                        End If
                    Else
                        otherAssycode = anInterface.assy2
                        ' exit if we donot need receivers
                        If anInterface.getAssyisSender(2) <> Sender Then
                            flag = False
                        End If

                    End If
                    ' get interface corresponding parts
                    If anInterface.status <> LCase("na") And flag Then
                        ' TODO: REIMPLEMENT
                        ' otherPartCollection = Me.allByAssyCode_Cartypes(otherAssycode, anInterface.Cartypes)
                        If Not otherPartCollection Is Nothing Then
                            For Each otherPart In otherPartCollection
                                ' check if otherPart has a hit in cartypes as this part
                                If Me.MatchWithCartypes(otherPart.GetCartypes) Then
                                    If Not aDir.ContainsKey(otherPart.PartID) Then
                                        InterfacingParts.Add(Item:=otherPart)
                                        aDir.Add(otherPart.PartID, value:=otherPart)
                                    End If
                                End If
                            Next otherPart
                        End If
                    End If
                Next anInterface

                getInterfacingParts = InterfacingParts
                Exit Function
            Else
                getInterfacingParts = Nothing
                Exit Function
            End If
        End Function

        '****** createDependencyFromInterfaces returns the clsOTDBDependency
        '******
        Public Function CreateDependencyFromInterfaces(ifcdepends As Scheduling.clsOTDBDependency) As Boolean
            Dim aColInterfaces As New Collection
            Dim anInterface As IFM.clsOTDBInterface
            Dim aCartypes As clsCartypes
            Dim ourAssyCode As String
            Dim otherAssycode As String
            Dim otherPartCollection As Collection
            Dim otherPart As Part
            Dim aDependM As New OnTrack.Scheduling.clsOTDBDependMember
            'Dim ifcdepends As New clsOTDBDependency
            Dim aDir As New Dictionary(Of String, Object)
            Dim flag As Boolean

            If _IsLoaded Then

                'get AssyCode of this Assy
                ourAssyCode = Me.GetAssycode()

                'get the interfaces
                aColInterfaces = Me.GetInterfaces()
                If aColInterfaces Is Nothing Then
                    CreateDependencyFromInterfaces = False
                    Exit Function
                End If

                ' our cartypes
                aCartypes = Me.GetCartypes

                ' go through all interfaces and get the parts
                For Each anInterface In aColInterfaces
                    flag = True    ' to cointue
                    ' we are pairno #1
                    If anInterface.assy1 = ourAssyCode Then
                        'if pairno #2 is the sender -> we are the receiver !
                        If anInterface.getAssyisSender(2) Then
                            flag = True
                            otherAssycode = anInterface.assy2
                            ' nor sender or receiver if r2
                        ElseIf anInterface.status = "r2" Then
                            flag = True
                            otherAssycode = anInterface.assy2
                        Else
                            flag = False
                        End If
                    Else
                        'we are pairno #2
                        'if pairno #2 is the receiver if pair 1 is the sender
                        If anInterface.getAssyisSender(1) Then
                            flag = True
                            otherAssycode = anInterface.assy1
                            ' nor sender or receiver if r2
                        ElseIf anInterface.status = "r2" Then
                            flag = True
                            otherAssycode = anInterface.assy1
                        Else
                            flag = False
                        End If
                    End If

                    ' get interface corresponding parts
                    If anInterface.status <> LCase("na") And flag Then
                        ' reimplement
                        ' otherPartCollection = Me.allByAssyCode_Cartypes(otherAssycode, anInterface.Cartypes)
                        If Not otherPartCollection Is Nothing Then
                            ' create the ifcdepends
                            If Not ifcdepends.IsCreated And Not ifcdepends.IsLoaded Then
                                ifcdepends.Create(Me.PartID)
                            End If
                            ' add the Interfacing Parts for each Interface
                            For Each otherPart In otherPartCollection
                                ' check if otherPart has a hit in cartypes as this part
                                If Me.MatchWithCartypes(otherPart.GetCartypes) Then
                                    aDependM = ifcdepends.AddPartID(typeid:=ConstDepTypeIDIFC, partid:=otherPart.PartID)
                                    If Not aDependM Is Nothing Then
                                        If anInterface.status <> "r2" Then
                                            aDependM.category = "receiver"
                                        Else
                                            aDependM.category = "bidirected"
                                        End If
                                        aDependM.condition = "IFC1"
                                        aDependM.parameter_num1 = anInterface.UID
                                        aDependM.parameter_txt1 = anInterface.status
                                        aDependM.parameter_num2 = anInterface.Cartypes.nousedCars
                                    End If
                                End If

                            Next otherPart
                        End If
                    End If
                Next anInterface

                If ifcdepends.NoMembers(ConstDepTypeIDIFC) > 0 Then
                    CreateDependencyFromInterfaces = True
                Else
                    CreateDependencyFromInterfaces = False
                End If
                Exit Function
            Else
                CreateDependencyFromInterfaces = False
                Exit Function
            End If
        End Function

        '****** getInterfaces returns the clsOTDBInterfaces to which this part has intefaces with
        '******
        Public Function GetInterfaces(Optional reload = False) As Collection
            Dim aCollection As Collection
            Dim assycode As String
            Dim selectCartypes As clsCartypes
            Dim anInterface As New IFM.clsOTDBInterface

            If reload Or s_interfaceCollection.Count = 0 Then
            End If

            If _IsLoaded Then
                selectCartypes = Me.GetCartypes
                If Me.GetCartypes.nousedCars = 0 Then
                    Call CoreMessageHandler(subname:="Part.getInterfaces", message:="cartypes are not selected for any car", break:=False)
                End If
                ' get the assycode in the form xx.xx.xx
                assycode = GetAssycode()

                aCollection = anInterface.allByAssyCode(assycode, selectCartypes)
                s_interfaceCollection = aCollection    'store the collection
                GetInterfaces = aCollection
                Exit Function
            Else
                GetInterfaces = Nothing
                Exit Function
            End If
        End Function
        '****** getDeliverables return the Documents in a Collection
        '******
        Public Function GetDeliverables() As List(Of Deliverable)
            If _IsLoaded Then
                ' get the Table from the Factory
                Return Deliverable.AllByPnid(partid:=Me.PartID)
            Else
                Return New List(Of Deliverable)
            End If
        End Function

        '************** matchWithCartypes: check if me.cartypes have at least one in common with anOthercartypes
        '**************
        Public Function MatchWithCartypes(anOthercartypes As clsCartypes) As Boolean


            Dim i As Integer
            Dim ourCartypes As clsCartypes

            If Not _IsLoaded And Not Me.IsCreated Then
                MatchWithCartypes = False
            End If

            ourCartypes = Me.GetCartypes
            For i = 1 To ourCartypes.getNoCars
                If ourCartypes.getCar(i) = anOthercartypes.getCar(i) And ourCartypes.getCar(i) = True Then
                    MatchWithCartypes = True
                    Exit Function
                End If
            Next i

            'return false
            MatchWithCartypes = False

        End Function

        '****** getCartypes of the part -> Document
        '******
        Public Function GetCartypes() As clsCartypes
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim pkarry() As Object
            Dim aCartypes As New clsCartypes
            Dim i As Integer
            Dim amount As Integer
            Dim fieldname As String


            If Not _IsLoaded Then
                GetCartypes = Nothing
                Exit Function
            End If

            ' set the primaryKey
            ReDim pkarry(0)
            If Me.DeliverableUID <> 0 Then
                pkarry(0) = Me.DeliverableUID
            Else
                Dim aCollection As List(Of Deliverable) = Deliverable.AllByPnid(partid:=Me.PartID)
                If aCollection.Count = 0 Then Debug.Assert(False)
                Dim aDeliverable As Deliverable = aCollection.Item(1)
                pkarry(0) = aDeliverable.Uid
            End If


            ''' HACK !
            aTable = GetTableStore("tblcartypes")
            aRecord = aTable.GetRecordByPrimaryKey(pkarry)

            If aRecord Is Nothing Then
                GetCartypes = Nothing
                Exit Function
            Else
                For i = 1 To aCartypes.getNoCars
                    fieldname = "ct" & Format(i, "0#")
                    amount = CInt(aRecord.GetValue(fieldname))
                    If amount > 0 Then Call aCartypes.addCartypeAmountByIndex(i, amount)
                Next i
                GetCartypes = aCartypes
                Exit Function
            End If


        End Function

        '********* getPrecode helper to create a Precode out of a PartID in the FORM 3HXX-YYYYYY-000 to 3.HXX
        '*********
        Public Function GetPrecode() As String

            If _IsLoaded Or Me.IsCreated Then
                GetPrecode = Mid(Me.PartID, 1, 1) & "." & UCase(Mid(Me.PartID, 2, 3)) & "-"
            Else
                GetPrecode = ""
            End If
        End Function


    End Class
End Namespace
