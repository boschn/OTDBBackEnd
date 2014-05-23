

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
Imports OnTrack.Commons

Namespace OnTrack.Parts

    ''' <summary>
    ''' part and assembly definition with reference link to deliverables
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=Part.ConstObjectID, description:="part and assembly definition with reference link to deliverables", _
        modulename:=ConstModuleParts, Version:=1, AdddeleteFieldBehavior:=True)> _
    Public Class Part
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "Part"

        '*** SCHEMA TABLE
        <ormSchemaTable(Version:=2)> Public Const ConstTableID As String = "tblParts"

        '*** Primary key
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, primarykeyOrdinal:=1, _
            XID:="pt1", Aliases:={"C10"}, title:="PartID", description:="unique ID of the part")> Public Const ConstFNPartID = "pnid"

        '** Indices
        <ormSchemaIndex(columnname1:=ConstFNIsDeleted, columnname2:=ConstFNPartID)> Public Const ConstIndexDeleted = "indDeleted"
        <ormSchemaIndex(columnname1:=constFNMatchCode, columnname2:=ConstFNPartID, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexMatchcode = "indmatchcode"
        <ormSchemaIndex(columnname1:=constFNCategory, columnname2:=ConstFNPartID, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexcategory = "indcategory"
        <ormSchemaIndex(columnname1:=constFNFunction, columnname2:=ConstFNPartID, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexFunction = "indFunction"
        <ormSchemaIndex(columnname1:=constFNTypeID, columnname2:=ConstFNPartID, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexType = "indType"
        <ormSchemaIndex(columnName1:=ConstFNDomainID, columnname2:=ConstFNPartID, columnname3:=ConstFNIsDeleted)> Public Const constIndexDomain = "indDomains"

        '*** Fields
        <ormObjectEntry(typeid:=otDataType.Text, size:=150, isnullable:=True, _
            XID:="pt2", Title:="Description", description:="description of the part")> Public Const ConstFNDescription = "desc"

        <ormObjectEntry(typeid:=otDataType.Text, size:=150, isnullable:=True, _
           XID:="pt3", aliases:={"DLV31"}, Title:="Workpackage", description:="workpackage of the part")> Public Const ConstFNWorkpackage = "wkpk"

        <ormObjectEntry(referenceobjectentry:=Commons.Workspace.ConstObjectID & "." & Commons.Workspace.ConstFNID, _
           Description:="workspaceID ID of the part")> Public Const ConstFNWorkspace = Commons.Workspace.ConstFNID

        <ormObjectEntry(referenceobjectentry:=Deliverables.Deliverable.ConstObjectID & "." & Deliverables.Deliverable.constFNUid, isnullable:=True, _
           XID:="DLV1", aliases:={"UID"}, Description:="deliverable UID of the part")> Public Const ConstFNDeliverableUID = Deliverables.Deliverable.constFNUid

        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, isnullable:=True, _
            XID:="pt4", Title:="Responsible", description:="responsible person for the deliverable", XID:="DLV16")> Public Const constFNResponsiblePerson = "resp"

        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, isnullable:=True, _
            XID:="pt5", title:="Responsible OrgUnit", description:=" organization unit responsible for the part", XID:="")> Public Const constFNRespOU = "respou"

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, isnullable:=True, _
            XID:="pt6", title:="Type", description:="type of the part", XID:="DLV13")> Public Const constFNTypeID = "typeid"

        <ormObjectEntry(typeid:=otDataType.Text, size:=150, isnullable:=True, _
            XID:="pt7", title:="Category", description:="category of the part", XID:="DLV13")> Public Const constFNCategory = "cat"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
            XID:="pt8", title:="blocking item reference", description:="blocking item reference id for the deliverable", aliases:={"DLV17"})> Public Const constFNBlockingItemReference = "blitemid"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
            XID:="pt9", aliases:={"dlv8"}, title:="Change Reference", description:="change reference of the deliverable")> Public Const constFNChangeRef = "chref"

        <ormObjectEntry(typeid:=otDataType.Memo, isnullable:=True, _
            XID:="pt10", title:="comment", description:="comments of the part", XID:="DLV18")> Public Const constFNComment = "cmt"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
            XID:="pt11", title:="Matchcode", description:="match code of the part")> Public Const constFNMatchCode = "matchcode"

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, isnullable:=True, _
             XID:="pt12", Title:="Function", description:="function of the deliverable")> Public Const constFNFunction = "function"


        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            isnullable:=True, _
            dbdefaultvalue:=ConstGlobalDomain, _
            useforeignkey:=otForeignKeyImplementation.ORM, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.SetDefault & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.Cascade & ")"})> _
        Public Const ConstFNDomain = "DOMAIN" '' different name since we donot want to get it deactivated due to missing domain behavior

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
             description:="not used and should be not active", _
          useforeignkey:=otForeignKeyImplementation.None)> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID  '' const not overidable

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
        <ormEntryMapping(EntryName:=constFNBlockingItemReference)> Private _blockingitemID As String
        <ormEntryMapping(EntryName:=constFNCategory)> Private _categoryID As String
        <ormEntryMapping(EntryName:=constFNMatchCode)> Private _matchcode As String
        <ormEntryMapping(EntryName:=ConstFNDomain)> Private _domainid As String
        ' dynamic
        Private s_interfaceCollection As New Collection




#Region "Properties"
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
        ''' sets or gets the domain id
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property Domainid() As String
            Get
                Return _domainid
            End Get
            Set(value As String)
                SetValue(ConstFNDomain, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the linkes Deliverable UID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DeliverableUID() As Long
            Get
                Return _deliverableUID
            End Get
            Set(value As Long)
                SetValue(ConstFNDeliverableUID, value)
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
                Return _workpackage
            End Get
            Set(value As String)
                SetValue(ConstFNWorkpackage, value)
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
                Return _workspaceID
            End Get
            Set(value As String)
                SetValue(ConstFNWorkspace, value)
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
                Return _description
            End Get
            Set(value As String)
                SetValue(ConstFNDescription, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the category
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CategoryID() As String
            Get
                Return _categoryID
            End Get
            Set(value As String)
                SetValue(constFNCategory, value)
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
                SetValue(constFNFunction, value)
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
                Return _respOU
            End Get
            Set(value As String)
                SetValue(constFNRespOU, value)
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
                Return _blockingitemID
            End Get
            Set(value As String)
                SetValue(constFNBlockingItemReference, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the Part-Type
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Property TypeID() As String
            Get
                Return _typeid
            End Get
            Set(value As String)
                SetValue(constFNTypeID, value)
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
                Return _matchcode
            End Get
            Set(value As String)
                SetValue(constFNMatchCode, value)
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
                Return _changerefID
            End Get
            Set(value As String)
                SetValue(constFNChangeRef, value)
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
                Return _comment
            End Get
            Set(value As String)
                SetValue(constFNComment, value)
            End Set
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
        Public Overloads Shared Function Retrieve(pnid As String) As Part
            Dim primarykey() As Object = {pnid}
            Return ormDataObject.Retrieve(Of Part)(primarykey)
        End Function
        ''' <summary>
        ''' Create an Object in the datastore
        ''' </summary>
        ''' <param name="partid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal partid As String, Optional domainid As String = "", Optional workspaceID As String = "") As Part
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID
            Dim aRecord As New ormRecord
            With aRecord
                .SetValue(ConstFNDomain, domainid)
                .SetValue(ConstFNWorkspace, workspaceID)
                .SetValue(ConstFNPartID, partid)
            End With
            Return ormDataObject.CreateDataObject(Of Part)(aRecord, domainID:=domainid, checkUnique:=True)

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

            'If me.isloaded Then
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

            If Me.IsLoaded Then
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
            If Me.IsLoaded Then
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
            Dim aCartypes As clsLEGACYCartypes
            Dim ourAssyCode As String
            Dim otherAssycode As String
            Dim otherPartCollection As Collection
            Dim otherPart As Part
            Dim InterfacingParts As New Collection
            Dim aDir As New Dictionary(Of String, Object)
            Dim flag As Boolean

            ''' rework
            Throw New NotImplementedException()


            If Me.IsLoaded Then

                ourAssyCode = Me.GetAssycode()
                'get the interfaces
                aColInterfaces = Me.GetInterfaces()
                If aColInterfaces Is Nothing Then
                    getInterfacingParts = Nothing
                    Exit Function
                End If
                aCartypes = Me.LEGACY_GetCartypes
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
                                If Me.LEGACY_MatchWithCartypes(otherPart.LEGACY_GetCartypes) Then
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
            Dim aCartypes As clsLEGACYCartypes
            Dim ourAssyCode As String
            Dim otherAssycode As String
            Dim otherPartCollection As Collection
            Dim otherPart As Part
            Dim aDependM As New OnTrack.Scheduling.clsOTDBDependMember
            'Dim ifcdepends As New clsOTDBDependency
            Dim aDir As New Dictionary(Of String, Object)
            Dim flag As Boolean

            If Me.IsLoaded Then

                'get AssyCode of this Assy
                ourAssyCode = Me.GetAssycode()

                'get the interfaces
                aColInterfaces = Me.GetInterfaces()
                If aColInterfaces Is Nothing Then
                    CreateDependencyFromInterfaces = False
                    Exit Function
                End If

                ' our cartypes
                aCartypes = Me.LEGACY_GetCartypes

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
                                If Me.LEGACY_MatchWithCartypes(otherPart.LEGACY_GetCartypes) Then
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
            Dim selectCartypes As clsLEGACYCartypes
            Dim anInterface As New IFM.clsOTDBInterface

            If reload Or s_interfaceCollection.Count = 0 Then
            End If

            If Me.IsLoaded Then
                selectCartypes = Me.LEGACY_GetCartypes
                If Me.LEGACY_GetCartypes.nousedCars = 0 Then
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
            If Me.IsLoaded Then
                ' get the Table from the Factory
                Return Deliverable.AllByPnid(partid:=Me.PartID)
            Else
                Return New List(Of Deliverable)
            End If
        End Function

        '************** matchWithCartypes: check if me.cartypes have at least one in common with anOthercartypes
        '**************
        Public Function LEGACY_MatchWithCartypes(anOthercartypes As clsLEGACYCartypes) As Boolean

            ''' LEGACY
            ''' 
            Throw New NotImplementedException

            'Dim i As Integer
            'Dim ourCartypes As clsLEGACYCartypes

            'If Not Me.IsLoaded And Not Me.IsCreated Then
            '    MatchWithCartypes = False
            'End If

            'ourCartypes = Me.GetCartypes
            'For i = 1 To ourCartypes.getNoCars
            '    If ourCartypes.getCar(i) = anOthercartypes.getCar(i) And ourCartypes.getCar(i) = True Then
            '        MatchWithCartypes = True
            '        Exit Function
            '    End If
            'Next i

            ''return false
            'MatchWithCartypes = False

        End Function

        '****** getCartypes of the part -> Document
        '******
        Public Function LEGACY_GetCartypes() As clsLEGACYCartypes

            ''' should be done via a configuration
            ''' getProperties
            Throw New NotImplementedException

            'Dim aTable As iormDataStore
            'Dim aRecord As ormRecord
            'Dim pkarry() As Object
            'Dim aCartypes As New clsLEGACYCartypes
            'Dim i As Integer
            'Dim amount As Integer
            'Dim fieldname As String


            'If Not Me.IsLoaded Then
            '    LEGACY_GetCartypes = Nothing
            '    Exit Function
            'End If

            '' set the primaryKey
            'ReDim pkarry(0)
            'If Me.DeliverableUID <> 0 Then
            '    pkarry(0) = Me.DeliverableUID
            'Else
            '    Dim aCollection As List(Of Deliverable) = Deliverable.AllByPnid(partid:=Me.PartID)
            '    If aCollection.Count = 0 Then Debug.Assert(False)
            '    Dim aDeliverable As Deliverable = aCollection.Item(1)
            '    pkarry(0) = aDeliverable.Uid
            'End If


            '''' HACK !
            'aTable = GetTableStore("tblcartypes")
            'aRecord = aTable.GetRecordByPrimaryKey(pkarry)

            'If aRecord Is Nothing Then
            '    LEGACY_GetCartypes = Nothing
            '    Exit Function
            'Else
            '    For i = 1 To aCartypes.getNoCars
            '        fieldname = "ct" & Format(i, "0#")
            '        amount = CInt(aRecord.GetValue(fieldname))
            '        If amount > 0 Then Call aCartypes.addCartypeAmountByIndex(i, amount)
            '    Next i
            '    LEGACY_GetCartypes = aCartypes
            '    Exit Function
            'End If


        End Function




    End Class
End Namespace
