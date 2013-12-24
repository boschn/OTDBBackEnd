

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
    '***** CLASS clsOTDBPart is the object for a OTDBRecord (which is the datastore)
    '*****
    '*****
    Public Class clsOTDBPart
        Inherits ormDataObject
        Implements iormInfusable
        Implements iotCloneable(Of clsOTDBPart)
        Implements iormPersistable

        Public Const constTableID As String = "tblParts"

        Public Const constFNPartID = "pnid"
        Public Const constFNSite = "site"
        Public Const constFNdept = "dept"
        Public Const constFNWkPk = "wkpk"

        Private s_description As String
        Private s_deliverableUID As Long
        Private s_partID As String    ' unique key
        Private s_site As String
        Private s_dept As String
        Private s_wkpk As String
        Private s_typeid As String
        Private s_da_area As String
        Private s_respOU As String
        Private s_delegateOU As String
        Private s_pole As String
        Private s_responsible As String
        Private s_chg As Date
        Private s_changerefID As String
        Private s_comment As String
        Private s_msglogtag As String
        Private s_blockingitemID As String
        Private s_category As String
        Private s_configtag As String

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

        ' dynamic
        Private s_interfaceCollection As New Collection
        Private s_isSender As Boolean
        Private s_isReceiver As Boolean



        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(constTableID)
        End Sub

        ''' <summary>
        ''' clone the object with the new primary key
        ''' </summary>
        ''' <param name="pkarray">primary key array</param>
        ''' <remarks></remarks>
        ''' <returns>the new cloned object or nothing</returns>
        Public Function Clone(pkarray() As Object) As clsOTDBPart Implements iotCloneable(Of clsOTDBPart).Clone
            Return MyBase.Clone(Of clsOTDBPart)(pkarray)
        End Function

        ''' <summary>
        ''' initialize
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean

            Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
            s_parameter_date1 = ConstNullDate
            s_parameter_date2 = ConstNullDate
            s_parameter_date3 = ConstNullDate
            Return MyBase.Initialize()
        End Function

#Region "properties"


        ReadOnly Property PartID() As String
            Get
                PartID = s_partID
            End Get

        End Property

        Public Property DeliverableUID() As Long
            Get
                DeliverableUID = s_deliverableUID
            End Get
            Set(value As Long)
                If s_deliverableUID <> value Then
                    s_deliverableUID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Description() As String
            Get
                Description = s_description
            End Get
            Set(value As String)
                If value <> s_description Then
                    s_description = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property dept() As String
            Get
                dept = s_dept
            End Get
            Set(value As String)
                If s_dept <> value Then
                    s_dept = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property site() As String
            Get
                site = s_site
            End Get
            Set(value As String)
                If value <> s_site Then
                    s_site = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property wkpk() As String
            Get
                wkpk = s_wkpk
            End Get
            Set(value As String)
                If value <> s_wkpk Then
                    s_wkpk = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property da_area() As String
            Get
                da_area = s_da_area
            End Get
            Set(value As String)
                If value <> s_da_area Then
                    s_da_area = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property category() As String
            Get
                category = s_category
            End Get
            Set(value As String)
                If value <> s_category Then
                    s_category = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property responsible() As String
            Get
                responsible = s_responsible
            End Get
            Set(value As String)
                If value <> s_responsible Then
                    s_responsible = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property responsibleOU() As String
            Get
                responsibleOU = s_respOU
            End Get
            Set(value As String)
                If value <> s_respOU Then
                    s_respOU = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property pole() As String
            Get
                pole = s_pole
            End Get
            Set(value As String)
                If value <> s_pole Then
                    s_pole = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property blockingItemID() As String
            Get
                blockingItemID = s_blockingitemID
            End Get
            Set(value As String)
                If s_blockingitemID <> value Then
                    s_blockingitemID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property


        Public Property isSender() As Boolean
            Get
                isSender = s_isSender
            End Get
            Set(value As Boolean)
                s_isSender = value
                'me.ischanged = True
            End Set
        End Property


        Public Property isReceiver() As Boolean
            Get
                isReceiver = s_isReceiver
            End Get
            Set(value As Boolean)
                s_isReceiver = value
                'me.ischanged = True
            End Set
        End Property

        Public Property parttype() As String
            Get
                parttype = s_typeid
            End Get
            Set(value As String)
                If s_typeid <> value Then
                    s_typeid = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ReadOnly Property ChangedOn() As Date
            Get
                ChangedOn = s_chg
            End Get

        End Property


        Public Property changeReferenceID() As String
            Get
                changeReferenceID = s_changerefID
            End Get
            Set(value As String)
                If s_changerefID <> value Then
                    s_changerefID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property comment() As String
            Get
                comment = s_comment
            End Get
            Set(value As String)
                s_comment = value
                Me.IsChanged = True
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

        '****** createTAG
        Public Function getUniqueTag()
            getUniqueTag = ConstDelimiter & constTableID & ConstDelimiter & s_partID & ConstDelimiter
        End Function
        ReadOnly Property Msglogtag() As String
            Get
                If s_msglogtag = "" Then
                    s_msglogtag = getUniqueTag()
                End If
                Msglogtag = s_msglogtag
            End Get

        End Property

        ReadOnly Property Configtag()
            Get
                If s_configtag = "" Then
                    s_configtag = getUniqueTag()
                End If
                Configtag = s_configtag
            End Get
        End Property
#End Region
        ''' <summary>
        ''' Infuse the data object by the record
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean Implements iormInfusable.Infuse

            '* init
            If Not Me.IsInitialized Then
                If Not Initialize() Then
                    Infuse = False
                    Exit Function
                End If
            End If


            Try
                s_deliverableUID = CLng(record.GetValue("dlvuid"))
                s_partID = CStr(record.GetValue("pnid"))
                s_dept = CStr(record.GetValue("dept"))
                s_site = CStr(record.GetValue("site"))
                s_wkpk = CStr(record.GetValue("wkpk"))
                s_da_area = CStr(record.GetValue("daar"))
                s_pole = CStr(record.GetValue("pole"))
                s_typeid = CStr(record.GetValue("typeid"))
                s_description = CStr(record.GetValue("desc"))
                s_responsible = CStr(record.GetValue("resp"))
                s_respOU = CStr(record.GetValue("respou"))
                s_comment = CStr(record.GetValue("cmt"))
                s_msglogtag = CStr(record.GetValue("msglogtag"))
                s_changerefID = CStr(record.GetValue("chref"))
                s_blockingitemID = CStr(record.GetValue("blitemid"))
                s_category = CStr(record.GetValue("cat"))
                s_configtag = CStr(record.GetValue("cnftag"))


                s_parameter_txt1 = CStr(record.GetValue("param_txt1"))
                s_parameter_txt2 = CStr(record.GetValue("param_txt2"))
                s_parameter_txt3 = CStr(record.GetValue("param_txt3"))
                s_parameter_num1 = CDbl(record.GetValue("param_num1"))
                s_parameter_num2 = CDbl(record.GetValue("param_num2"))
                s_parameter_num3 = CDbl(record.GetValue("param_num3"))
                s_parameter_date1 = CDate(record.GetValue("param_date1"))
                s_parameter_date2 = CDate(record.GetValue("param_date2"))
                s_parameter_date3 = CDate(record.GetValue("param_date3"))
                s_parameter_flag1 = CBool(record.GetValue("param_flag1"))
                s_parameter_flag2 = CBool(record.GetValue("param_flag2"))
                s_parameter_flag3 = CBool(record.GetValue("param_flag3"))

                If Not IsNull(record.GetValue(ConstFNDeletedOn)) Then
                    _deletedOn = CDate(record.GetValue(ConstFNDeletedOn))
                End If
                Me.IsDeleted = CBool(record.GetValue(ConstFNIsDeleted))

                s_chg = CDate(record.GetValue("chg"))

                Return MyBase.Infuse(record)

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBPart.Infuse")
                Return False
            End Try



        End Function

        ''' <summary>
        ''' Update the Record from the object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function UpdateRecord() As Boolean

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    UpdateRecord = False
                    Exit Function
                End If
            End If

            Try
                Call Me.Record.SetValue("daar", s_da_area)
                Call Me.Record.SetValue("wkpk", s_wkpk)
                Call Me.Record.SetValue("site", s_site)
                Call Me.Record.SetValue("dept", s_dept)
                'Call me.record.setValue("cust", s_customerID)
                Call Me.Record.SetValue("pnid", s_partID)
                Call Me.Record.SetValue("desc", s_description)
                Call Me.Record.SetValue("cmt", s_comment)
                Call Me.Record.SetValue("pole", s_pole)
                Call Me.Record.SetValue("chref", s_changerefID)
                Call Me.Record.SetValue("dlvuid", s_deliverableUID)
                Call Me.Record.SetValue("respou", s_respOU)
                Call Me.Record.SetValue("resp", s_responsible)
                Call Me.Record.SetValue("typeid", s_typeid)
                Call Me.Record.SetValue("cat", s_category)
                Call Me.Record.SetValue("blitemid", s_blockingitemID)
                Call Me.Record.SetValue("cnftag", s_configtag)

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


                Return True

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBPart.UpdateRecord")
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Persist the object to the datastore
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Persist = False
                    Exit Function
                End If
            End If

            If Not UpdateRecord() Then
                Return False
            End If

            Return MyBase.Persist(timestamp)

        End Function


        '**** all returns all parts
        '****
        Public Function all(Optional isDeleted As Boolean = False) As Collection
            Dim aCollection As New Collection
            Dim aRecordCollection As List(Of ormRecord)
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim wherestr As String
            Dim orderbystr As String
            Dim aNewPart As clsOTDBPart

            ' param
            wherestr = "tblparts.pnid <> '' and tblparts.isdeleted = "
            If isDeleted Then
                wherestr = wherestr & "true"
            Else
                wherestr = wherestr & "false"
            End If
            ' order
            orderbystr = " tblparts.pnid asc "

            On Error GoTo error_handler

            aTable = GetTableStore(constTableID)
            aRecordCollection = aTable.GetRecordsBySql(wherestr:=wherestr, orderby:=orderbystr, silent:=True)

            If aRecordCollection Is Nothing Then
                Me.Unload()
                all = Nothing
                Exit Function
            Else
                For Each aRecord In aRecordCollection
                    aNewPart = New clsOTDBPart
                    If aNewPart.Infuse(aRecord) Then
                        aCollection.Add(Item:=aNewPart)
                    End If
                Next aRecord
                all = aCollection
                Exit Function
            End If

error_handler:

            all = Nothing
            Exit Function
        End Function

        '**** allByUID returns allByUID parts
        '****
        Public Function allByUID(ByVal deliverableUID As Long, Optional ByVal isDeleted As Boolean = False) As Collection
            Dim aCollection As New Collection
            Dim aRecordCollection As List(Of ormRecord)
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim wherestr As String
            Dim orderbystr As String
            Dim aNewPart As clsOTDBPart

            ' param
            wherestr = "tblparts.pnid <> '' and tblparts.deliverableUID =" & deliverableUID & " and tblparts.isdeleted = "
            If isDeleted Then
                wherestr = wherestr & "true"
            Else
                wherestr = wherestr & "false"
            End If
            ' order
            orderbystr = " tblparts.pnid asc "

            On Error GoTo error_handler

            aTable = GetTableStore(constTableID)
            aRecordCollection = aTable.GetRecordsBySql(wherestr:=wherestr, orderby:=orderbystr, silent:=True)

            If aRecordCollection Is Nothing Then
                Me.Unload()
                allByUID = Nothing
                Exit Function
            Else
                For Each aRecord In aRecordCollection
                    aNewPart = New clsOTDBPart
                    If aNewPart.Infuse(aRecord) Then
                        aCollection.Add(Item:=aNewPart)
                    End If
                Next aRecord
                allByUID = aCollection
                Exit Function
            End If

error_handler:

            allByUID = Nothing
            Exit Function
        End Function

        '**** allByAssyCode_Cartypes returns all parts with Assycode and one of the selected cartypes
        '****
        Public Function allByAssyCode_Cartypes(assycode As String, ByRef selectCartypes As clsCartypes) As Collection
            Dim aCollection As New Collection
            Dim aRecordCollection As List(Of ormRecord)
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim wherestr As String
            Dim i As Integer
            Dim flag As Boolean
            Dim substrings() As String
            Dim substr As String
            Dim innerjoin As String
            Dim aDir As New Dictionary(Of String, Object)
            Dim aNewPart As clsOTDBPart

            ' creats the whereclause
            If InStr(assycode, ".") > 0 Then
                substrings = Split(assycode, ".")
                substr = substrings(0) & substrings(1) & substrings(2)
            Else
                substr = assycode
            End If
            wherestr = "mid(tblparts.pnid, 6,6) = '" & substr & "' and mid(tblparts.pnid, 12,4) = '-000' and ("
            For i = 1 To selectCartypes.getNoCars

                If selectCartypes.getCar(i) Then
                    If flag Then
                        wherestr = wherestr & " or "
                    End If
                    wherestr = wherestr & "tblcartypes.ct" & Format(i, "0#") & "="
                    wherestr = wherestr & "true"

                    flag = True
                Else
                    'wherestr = wherestr & "false"
                End If

            Next i
            If flag Then
                wherestr = wherestr & ")"
            Else
                System.Diagnostics.Debug.WriteLine("clsOTDBParts.allByAssyCode: selectCartypes has no cartypes to select on")
                Call CoreMessageHandler(message:="selectCartypes has no cartypes to select on", _
                                           arg1:=Me.DeliverableUID & " " & assycode & " on " & selectCartypes.show, _
                                           subname:="clsOTDBPart.allByAssyCode_Cartypes", break:=False)
                'GoTo error_handler
            End If

            ' inner join
            innerjoin = " inner join tblcartypes on tblparts.dlvuid = tblcartypes.uid "
            'Debug.Print wherestr

            On Error GoTo error_handler

            aTable = GetTableStore(constTableID)
            aRecordCollection = aTable.GetRecordsBySql(wherestr, innerjoin:=innerjoin, silent:=True)

            If aRecordCollection Is Nothing Then
                Me.Unload()
                allByAssyCode_Cartypes = Nothing
                Exit Function
            Else
                For Each aRecord In aRecordCollection
                    aNewPart = New clsOTDBPart
                    If aNewPart.Infuse(aRecord) Then
                        If Not aDir.ContainsKey(aNewPart.PartID) Then
                            aCollection.Add(Item:=aNewPart)
                            aDir.Add(key:=aNewPart.PartID, value:=aNewPart)
                        End If
                    End If
                Next aRecord
                allByAssyCode_Cartypes = aCollection
                Exit Function
            End If

error_handler:

            allByAssyCode_Cartypes = Nothing
            Exit Function
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

            aTable = GetTableStore(constTableID)
            ' get rid of the '.'
            'precode = RemoveChar(precode, ".")
            textstr = Mid(precode, 1, 1) & Mid(precode, 3, 3)

            wherestr = "mid(" & constTableID & ".pnid,1,4) ='" & textstr & "' "
            ' select
            If department <> "" Then
                wherestr = wherestr & " and " & constTableID & ".dept ='" & department & "' "
            End If
            If site <> "" Then
                wherestr = wherestr & " and " & constTableID & ".site ='" & site & "' "
            End If
            If workpackage <> "" Then
                wherestr = wherestr & " and " & constTableID & ".wkpk ='" & workpackage & "' "
            End If

            aRecordCollection = aTable.GetRecordsBySql(wherestr:=wherestr)

            If aRecordCollection Is Nothing Then
                Me.Unload()
                AllByPrecodeAndOU = Nothing
                Exit Function
            Else
                For Each aRecord In aRecordCollection
                    Dim aNewPart As New clsOTDBPart
                    aNewPart = New clsOTDBPart
                    If aNewPart.Infuse(aRecord) Then
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
        Public Function LoadBy(pnid As String) As Boolean
            Dim primarykey() As Object = {pnid}
            Return MyBase.LoadBy(primarykey)
        End Function

        ''' <summary>
        ''' create persistency Schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean

            Dim aFieldDesc As New ormFieldDescription
            Dim PrimaryColumnNames As New Collection
            Dim aTable As New ObjectDefinition


            aFieldDesc.ID = ""
            aFieldDesc.Parameter = ""
            aFieldDesc.Relation = New String() {}
            aFieldDesc.Aliases = New String() {}
            aFieldDesc.Tablename = constTableID

            With aTable
                .Create(constTableID)
                .Delete()

                '***
                '*** Fields
                '****

                'Type
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "partid"
                aFieldDesc.ColumnName = constFNPartID
                aFieldDesc.Aliases = New String() {"c10"}
                aFieldDesc.ID = "pt1"
                aFieldDesc.Size = 50
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "category"
                aFieldDesc.ColumnName = "cat"
                aFieldDesc.ID = "pt2"
                aFieldDesc.Aliases = New String() {}
                aFieldDesc.Size = 50
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "site"
                aFieldDesc.ColumnName = constFNSite
                aFieldDesc.Aliases = New String() {"c7"}
                aFieldDesc.ID = "pt3"
                aFieldDesc.Size = 50
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "department"
                aFieldDesc.ColumnName = constFNdept
                aFieldDesc.ID = "pt4"
                aFieldDesc.Aliases = New String() {"c8"}
                aFieldDesc.Size = 50
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "workpackage delegated site"
                aFieldDesc.ColumnName = constFNWkPk
                aFieldDesc.ID = "pt5"
                aFieldDesc.Aliases = New String() {"c9"}
                aFieldDesc.Size = 50
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "type of part"
                aFieldDesc.ColumnName = "typeid"
                aFieldDesc.ID = "pt6"
                aFieldDesc.Aliases = New String() {"c11"}
                aFieldDesc.Size = 50
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "design assurance area"
                aFieldDesc.ColumnName = "daar"
                aFieldDesc.ID = "pt7"
                aFieldDesc.Aliases = New String() {"c13"}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "change reference tag"
                aFieldDesc.ColumnName = "chref"
                aFieldDesc.ID = "pt8"
                aFieldDesc.Aliases = New String() {}
                aFieldDesc.Size = 50
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "responsible"
                aFieldDesc.ColumnName = "resp"
                aFieldDesc.ID = "pt9"
                aFieldDesc.Aliases = New String() {"c14"}
                aFieldDesc.Size = 100
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "description"
                aFieldDesc.ColumnName = "desc"
                aFieldDesc.ID = "pt10"
                aFieldDesc.Aliases = New String() {"c6"}
                aFieldDesc.Size = 0
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "responsible OU name"
                aFieldDesc.ColumnName = "respou"
                aFieldDesc.ID = "pt11"
                aFieldDesc.Aliases = New String() {}
                aFieldDesc.Size = 50
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Date]
                aFieldDesc.Title = "last change date"
                aFieldDesc.ColumnName = "chg"
                aFieldDesc.ID = "pt12"
                aFieldDesc.Aliases = New String() {}
                aFieldDesc.Size = 0
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "blocking item"
                aFieldDesc.ColumnName = "blitemid"
                aFieldDesc.ID = "pt17"
                aFieldDesc.Aliases = New String() {"bs5"}
                aFieldDesc.Size = 50
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' dlvUID
                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "deliverable UID"
                aFieldDesc.ColumnName = "dlvuid"
                aFieldDesc.ID = "pt20"
                aFieldDesc.Aliases = New String() {"uid"}
                aFieldDesc.Size = 0
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' cmt
                aFieldDesc.Datatype = otFieldDataType.Memo
                aFieldDesc.Title = "comments"
                aFieldDesc.ColumnName = "cmt"
                aFieldDesc.ID = "pt18"
                aFieldDesc.Size = 0
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                '**** configtag
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "tag of config"
                aFieldDesc.ID = "pt19"
                aFieldDesc.Aliases = New String() {"cnfl4"}
                aFieldDesc.Size = 100
                aFieldDesc.ColumnName = "cnftag"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' msglogtag
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "message log tag"
                aFieldDesc.ColumnName = "msglogtag"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_txt 1
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "parameter_txt 1 of condition"
                aFieldDesc.ColumnName = "param_txt1"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_txt 2
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "parameter_txt 2 of condition"
                aFieldDesc.ColumnName = "param_txt2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_txt 2
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "parameter_txt 3 of condition"
                aFieldDesc.ColumnName = "param_txt3"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_num 1
                aFieldDesc.Datatype = otFieldDataType.Numeric
                aFieldDesc.Title = "parameter numeric 1 of condition"
                aFieldDesc.ColumnName = "param_num1"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_num 2
                aFieldDesc.Datatype = otFieldDataType.Numeric
                aFieldDesc.Title = "parameter numeric 2 of condition"
                aFieldDesc.ColumnName = "param_num2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' parameter_num 2
                aFieldDesc.Datatype = otFieldDataType.Numeric
                aFieldDesc.Title = "parameter numeric 3 of condition"
                aFieldDesc.ColumnName = "param_num3"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_date 1
                aFieldDesc.Datatype = otFieldDataType.[Date]
                aFieldDesc.Title = "parameter date 1 of condition"
                aFieldDesc.ColumnName = "param_date1"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_date 2
                aFieldDesc.Datatype = otFieldDataType.[Date]
                aFieldDesc.Title = "parameter date 2 of condition"
                aFieldDesc.ColumnName = "param_date2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' parameter_date 3
                aFieldDesc.Datatype = otFieldDataType.[Date]
                aFieldDesc.Title = "parameter date 3 of condition"
                aFieldDesc.ColumnName = "param_date3"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' parameter_flag 1
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "parameter flag 1 of condition"
                aFieldDesc.ColumnName = "param_flag1"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_flag 2
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "parameter flag 2 of condition"
                aFieldDesc.ColumnName = "param_flag2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' parameter_flag 3
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "parameter flag 3 of condition"
                aFieldDesc.ColumnName = "param_flag3"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                aFieldDesc.Aliases = New String() {}
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.ColumnName = ConstFNCreatedOn
                aFieldDesc.Aliases = New String() {}
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "deletion Date"
                aFieldDesc.ColumnName = ConstFNDeletedOn
                aFieldDesc.Aliases = New String() {}
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_flag 1
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "Deleted"
                aFieldDesc.Description = "flag if field is deleted"
                aFieldDesc.ColumnName = ConstFNIsDeleted
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

            '* reload the tablestore
            If CurrentSession.IsRunning Then
                CurrentSession.CurrentDBDriver.GetTableStore(tableID:=constTableID, force:=True)
            End If
            ' Handle the error
error_handle:
            Call CoreMessageHandler(subname:="clsOTDBdlviverable.createSchema")
            CreateSchema = False
        End Function

        ''' <summary>
        ''' Create an Object in the datastore
        ''' </summary>
        ''' <param name="partid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal partid As String) As Boolean
            Dim primarykey() As Object = {partid}
            If MyBase.Create(primarykey, checkUnique:=True) Then
                ' set the primaryKey
                s_partID = partid
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
            '    If Not getDeliverable.loadBy(Me.partid) Then
            '        Set getDeliverable = Nothing
            '    End If
            '    Exit Function
            'Else
            '    Set getDeliverable = Nothing
            '    Exit Function
            'End If

            Dim aCollection As Collection
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

        '****** getinterfacingParts returns the clsOTDBparts to this part has interfaces with
        '******
        Public Function getInterfacingParts(Optional Sender As Boolean = True, Optional Receiver As Boolean = True) As Collection
            Dim aColInterfaces As New Collection
            Dim anInterface As IFM.clsOTDBInterface
            Dim aCartypes As clsCartypes
            Dim ourAssyCode As String
            Dim otherAssycode As String
            Dim otherPartCollection As Collection
            Dim otherPart As clsOTDBPart
            Dim InterfacingParts As New Collection
            Dim aDir As New Dictionary(Of String, Object)
            Dim flag As Boolean

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
                        otherPartCollection = Me.allByAssyCode_Cartypes(otherAssycode, anInterface.Cartypes)
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
        Public Function CreateDependencyFromInterfaces(ifcdepends As scheduling.clsOTDBDependency) As Boolean
            Dim aColInterfaces As New Collection
            Dim anInterface As IFM.clsOTDBInterface
            Dim aCartypes As clsCartypes
            Dim ourAssyCode As String
            Dim otherAssycode As String
            Dim otherPartCollection As Collection
            Dim otherPart As clsOTDBPart
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
                        otherPartCollection = Me.allByAssyCode_Cartypes(otherAssycode, anInterface.Cartypes)
                        If Not otherPartCollection Is Nothing Then
                            ' create the ifcdepends
                            If Not ifcdepends.IsCreated And Not ifcdepends.IsLoaded Then
                                ifcdepends.create(Me.PartID)
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
                    Call CoreMessageHandler(subname:="clsOTDBPart.getInterfaces", message:="cartypes are not selected for any car", break:=False)
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
        Public Function GetDeliverables() As Collection
            Dim aCollection As Collection
            Dim aDocument As Deliverable

            If _IsLoaded Then
                ' get the Table from the Factory
                aDocument = New Deliverable
                aCollection = aDocument.AllByPnid(Me.PartID)
                GetDeliverables = aCollection
                Exit Function
            Else
                GetDeliverables = Nothing
                Exit Function
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
                Dim aCollection As Collection = Deliverable.AllByPnid(partid:=Me.PartID)
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
