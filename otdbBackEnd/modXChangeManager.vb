
REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** XChange Manager Module: static database backend functions (independent from Application such as EXCEL)
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

Imports System.Collections.Generic
Imports System.Diagnostics.Debug

Imports OnTrack
Namespace Ontrack

    Namespace XChange

        Public Module XChangeManager


            ' ***************************************************************************************************
            '   Module for OnTrack DB Exchange Manager (Interface and Mappings)
            '
            '   Author: B.Schneider
            '   created: 2013-04-01
            '
            '   change-log:
            ' ***************************************************************************************************


            '********** createXChangeConfigs
            '**********
            Public Sub createXChangeConfigs()
                Dim aConfig As clsOTDBXChangeConfig

                '**** XSTATUS -> Config to eXchange the Status
                '****
                aConfig = CreateXChangeConfigFromTable(configname:="xstatus", _
                                                           objectname:="tblDefStatusItems", xcmd:=otXChangeCommandType.Read)
                If aConfig Is Nothing Then
                End If

            End Sub

            '********** createXChangeConfigFromTable
            '**********
            Public Function CreateXChangeConfigFromTable(ByVal configname As String, _
                                                         ByVal objectname As String, _
                                                         ByVal xcmd As otXChangeCommandType) As clsOTDBXChangeConfig
                Dim anObjectName As String
                Dim aNewConfig As New clsOTDBXChangeConfig
                Dim aSchemaDefTable As New ObjectDefinition
                Dim i As Long

                '*** load the table definition
                If Not aSchemaDefTable.LoadBy(objectname) Then
                    Call ot.CoreMessageHandler(arg1:=objectname, tablename:=objectname, message:=" Could not load SchemaTableDefinition")
                    CreateXChangeConfigFromTable = Nothing
                    Exit Function
                End If

                '****
                '****
                anObjectName = objectname
                If aNewConfig.LoadBy(configname) Then
                    aNewConfig.Delete()
                End If

                ' create config
                aNewConfig.Create(configname)
                aNewConfig.AddObjectByName(anObjectName)
                i = 1
                '
                For Each aFieldDef As ObjectEntryDefinition In aSchemaDefTable.Entries
                    If aFieldDef.ID <> "" Then
                        Call aNewConfig.AddAttributeByField(objectentry:=aFieldDef, ordinal:=New Ordinal(i), xcmd:=xcmd)
                        i = i + 1
                    End If
                Next

                CreateXChangeConfigFromTable = aNewConfig
            End Function

            '********** createXChangeConfigFromIDs: creates a config from an array with IDs, ordinal will be the columns
            '**********
            Public Function createXChangeConfigFromIDs(ByVal CONFIGNAME As String, _
                                                       ByVal IDs As Object, _
                                                       ByVal XCMD As otXChangeCommandType, _
                                                       Optional ByRef OBJECTNAMES As Object = Nothing) As clsOTDBXChangeConfig
                Dim anObjectName As String
                Dim aNewConfig As New clsOTDBXChangeConfig
                Dim aColl As Collection
                Dim aSchemaDefTable As New ObjectDefinition
                Dim m As Object
                Dim aFieldDef As New ObjectEntryDefinition
                Dim i As Long

                '*** load the table definition
                'If Not aSchemaDefTable.loadBy(Tablename) Then
                '    Call OTDBErrorHandler(arg1:=Tablename, Tablename:=Tablename, message:=" Could not load SchemaTableDefinition")
                '    Set createXChangeConfigFromIDs = Nothing
                '    Exit Function
                'End If
                'anObjectName = Tablename
                'If aNewConfig.loadBy(ConfigName) Then
                '    aNewConfig.delete
                'End If

                ' create config
                aNewConfig.Create(CONFIGNAME)
                i = 0

                ' add Objectnames
                If IsArrayInitialized(OBJECTNAMES) Then
                    For i = LBound(OBJECTNAMES) To UBound(OBJECTNAMES)
                        Call aNewConfig.AddObjectByName(Name:=CStr(OBJECTNAMES(i)), orderno:=i, XCMD:=XCMD)
                    Next i
                ElseIf Not IsEmpty(OBJECTNAMES) Then
                    Call aNewConfig.AddObjectByName(Name:=CStr(OBJECTNAMES), orderno:=1, XCMD:=XCMD)
                End If

                For i = LBound(IDs) To UBound(IDs)
                    ' load ID
                    If Not IsEmpty(IDs(i)) Then
                        Call aNewConfig.AddAttributeByID(id:=IDs(i), ordinal:=i, isXChanged:=True, xcmd:=XCMD)
                        'Set aColl = aFieldDef.allByID(IDs(i))
                        'For Each m In aColl
                        '    Set aFieldDef = m
                        '    'Call aNewConfig.addObjectByName(aFieldDef.tablename, xcmd:=xcmd) -> by AttributesField
                        '    Call aNewConfig.addAttributeByField(FIELDENTRY:=aFieldDef, ordinal:=i, XCMD:=XCMD)
                        'Next m
                    End If
                Next i

                createXChangeConfigFromIDs = aNewConfig
            End Function

            '******* XChangeWith2DArray : eXchanges Data according the Config with an 2dimensional array
            '*******
            Public Function XChangeWith2DArray(ByRef CONFIG As clsOTDBXChangeConfig, _
                                               ByRef ARRAYDATA As Object) As Boolean
                Dim i As Long
                Dim rowno As Long

                Dim aMapping As New Dictionary(Of Object, Object)
                Dim listofAttributes As New Collection
                Dim Value As Object
                Dim CONFIGmember As New clsOTDBXChangeMember

                listofAttributes = CONFIG.Attributes
                If listofAttributes.Count = 0 Then
                    XChangeWith2DArray = False
                    Exit Function
                End If


                ' is Array initialized ?!
                'If Not ArrayIsInitializedV(ArrayData) Then
                '    redim Array
                'End If

                ' go through all rows of the Data
                i = 0
                For rowno = LBound(ARRAYDATA, 1) To UBound(ARRAYDATA, 1)
                    ' fetch the row
                    aMapping = New Dictionary(Of Object, Object)
                    For Each CONFIGmember In listofAttributes
                        If CONFIGmember.ISXCHANGED Then
                            If IsNumeric(CONFIGmember.ordinal) Then
                                i = CLng(CONFIGmember.ordinal.Value)
                            Else
                                i = i + 1
                            End If
                            If Not aMapping.ContainsKey(key:=i) Then
                                Call aMapping.Add(key:=i, value:=Trim(ARRAYDATA(rowno, i)))
                            End If
                        End If
                    Next CONFIGmember

                    'If Not PROGRESSBAR Is Nothing Then Call PROGRESSBAR.progress(1, Statustext:="updating row no" & rowno)
                    ' run the XChange with OTDB
                    Call CONFIG.RunXChange(aMapping:=aMapping)

                    ' save the row
                    i = 0
                    For Each CONFIGmember In listofAttributes
                        If CONFIGmember.ISXCHANGED Then
                            If IsNumeric(CONFIGmember.ordinal.Value) Then
                                i = CLng(CONFIGmember.ordinal.Value)
                            Else
                                i = i + 1
                            End If
                            If aMapping.ContainsKey(key:=i) Then
                                Value = aMapping.Item(key:=i)
                                ARRAYDATA(rowno, i) = Value
                            End If
                        End If
                    Next CONFIGmember

                Next rowno


            End Function

            '******* XChangeWithArray : eXchanges Data according the Config with an 2dimensional array
            '*******
            Public Function XChangeWithArray(ByRef aConfig As clsOTDBXChangeConfig, _
                                             ByRef ARRAYDATA() As Object) As Boolean
                Dim i As Long
                Dim rowno As Long

                Dim aMapping As New Dictionary(Of Object, Object)
                Dim listofAttributes As New Collection
                Dim Value As Object
                Dim aConfigmember As New clsOTDBXChangeMember

                listofAttributes = aConfig.Attributes

                If listofAttributes.Count = 0 Then
                    XChangeWithArray = False
                    Exit Function
                End If

                ' is Array initialized ?!
                If Not IsArrayInitialized(ARRAYDATA) Then
                    ReDim Preserve ARRAYDATA(listofAttributes.Count)
                End If

                ' fetch the row
                aMapping = New Dictionary(Of Object, Object)
                i = 0
                For Each aConfigmember In listofAttributes

                    If aConfigmember.ISXCHANGED Then
                        If IsNumeric(aConfigmember.ordinal.Value) Then
                            i = CLng(aConfigmember.ordinal.Value)
                        Else
                            i = i + 1
                        End If
                        If Not aMapping.ContainsKey(key:=i) Then
                            If i >= LBound(ARRAYDATA) And i <= UBound(ARRAYDATA) Then Call aMapping.Add(key:=i, value:=ARRAYDATA(i)) ' EMPTY possible
                        End If
                    End If
                Next aConfigmember

                ' run the XChange with OTDB
                On Error Resume Next
                Call aConfig.RunXChange(aMapping:=aMapping)

                ' save the row
                i = 0
                For Each aConfigmember In listofAttributes
                    If aConfigmember.IsXChanged Then
                        If IsNumeric(aConfigmember.ordinal.Value) Then
                            i = CLng(aConfigmember.ordinal.Value)
                        Else
                            i = i + 1
                        End If
                        If aMapping.ContainsKey(key:=i) Then
                            Value = aMapping.Item(key:=i)
                            If i >= LBound(ARRAYDATA) And i <= UBound(ARRAYDATA) Then ARRAYDATA(i) = Value
                        End If
                    End If
                Next aConfigmember




            End Function
        End Module

    End Namespace

End Namespace