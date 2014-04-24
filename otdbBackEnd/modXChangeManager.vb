
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
Imports OnTrack.Database

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
                Dim aConfig As XChangeConfiguration

                '**** XSTATUS -> Config to eXchange the Status
                '****
                aConfig = CreateXChangeConfigFromObjectDefinition(configname:="xstatus", _
                                                           objectname:="tblDefStatusItems", xcmd:=otXChangeCommandType.Read)
                If aConfig Is Nothing Then
                End If

            End Sub

            ''' <summary>
            ''' routine creates a xchange config from the object definition and adds all 
            ''' </summary>
            ''' <param name="configname"></param>
            ''' <param name="objectname"></param>
            ''' <param name="xcmd"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function CreateXChangeConfigFromObjectDefinition(ByVal configname As String, _
                                                         ByVal objectname As String, _
                                                         ByVal xcmd As otXChangeCommandType) As XChangeConfiguration
                Dim anObjectName As String
                Dim aNewConfig As XChangeConfiguration = XChangeConfiguration.Create(configname:=configname)
                If aNewConfig Is Nothing Then aNewConfig = XChangeConfiguration.Retrieve(configname:=configname)
                If aNewConfig Is Nothing Then
                    ot.CoreMessageHandler(message:="xchange configuration couldnot be created nor retrieved", arg1:=configname, subname:="XChangeManager.CreateXChangeConfigFromIDs")
                    Return Nothing
                End If
                Dim anObjectDefinition As ObjectDefinition = CurrentSession.Objects.GetObject(objectid:=objectname)
                Dim i As Long

                '*** load the table definition
                If anObjectDefinition Is Nothing Then
                    Call ot.CoreMessageHandler(arg1:=objectname, tablename:=objectname, message:=" Could not load ObjectDEFINITION")
                    CreateXChangeConfigFromObjectDefinition = Nothing
                    Exit Function
                End If

                '****
                '****
                anObjectName = objectname
                If aNewConfig IsNot Nothing Then
                    aNewConfig.Delete()
                End If

                ' create config
                aNewConfig = XChangeConfiguration.Create(configname)
                aNewConfig.AddObjectByName(anObjectName)
                i = 1
                '
                For Each aFieldDef As AbstractEntryDefinition In anObjectDefinition.GetEntries
                    If aFieldDef.XID <> "" Then
                        Call aNewConfig.AddEntryByObjectEntry(objectentry:=aFieldDef, ordinal:=New OnTrack.Database.Ordinal(i), xcmd:=xcmd)
                        i = i + 1
                    End If
                Next

                CreateXChangeConfigFromObjectDefinition = aNewConfig
            End Function

           
            ''' <summary>
            ''' creates a xchange configuration from a array of strings
            ''' </summary>
            ''' <param name="CONFIGNAME"></param>
            ''' <param name="IDs"></param>
            ''' <param name="XCMD"></param>
            ''' <param name="OBJECTNAMES"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function CreateXChangeConfigFromIDs(ByVal configname As String, _
                                                       ByVal xids As String(), _
                                                       ByVal xcmd As otXChangeCommandType, _
                                                       Optional ByRef objectids As String() = Nothing) As XChangeConfiguration

                Dim aNewConfig As XChangeConfiguration = XChangeConfiguration.Create(configname:=configname)
                If aNewConfig Is Nothing Then aNewConfig = XChangeConfiguration.Retrieve(configname:=configname)
                If aNewConfig Is Nothing Then
                    ot.CoreMessageHandler(message:="xchange configuration couldnot be created nor retrieved", arg1:=configname, subname:="XChangeManager.CreateXChangeConfigFromIDs")
                    Return Nothing
                End If

                Dim i As Long = 0

                ' add Objectnames
                If objectids IsNot Nothing Then
                    For i = LBound(objectids) To UBound(objectids)
                        Call aNewConfig.AddObjectByName(name:=CStr(objectids(i)), orderno:=i, xcmd:=xcmd)
                    Next i
                End If


                For i = LBound(xids) To UBound(xids)
                    ' load ID
                    If Not IsEmpty(xids(i)) Then
                        Call aNewConfig.AddEntryByXID(Xid:=xids(i), ordinal:=i, isXChanged:=True, xcmd:=xcmd)
                        'Set aColl = aFieldDef.allByID(IDs(i))
                        'For Each m In aColl
                        '    Set aFieldDef = m
                        '    'Call aNewConfig.addObjectByName(aFieldDef.tablename, xcmd:=xcmd) -> by AttributesField
                        '    Call aNewConfig.addAttributeByField(FIELDENTRY:=aFieldDef, ordinal:=i, XCMD:=XCMD)
                        'Next m
                    End If
                Next i

                CreateXChangeConfigFromIDs = aNewConfig
            End Function

            '******* XChangeWithArray : eXchanges Data according the Config with an 2dimensional array
            '*******
            Public Function XChangeWithArray(ByRef aConfig As XChangeConfiguration, _
                                             ByRef ARRAYDATA() As Object) As Boolean
                Dim i As Long
                Dim rowno As Long

                Dim aMapping As New Dictionary(Of Object, Object)
                Dim listofAttributes As New Collection
                Dim Value As Object
                Dim aConfigmember As New XChangeObjectEntry

                listofAttributes = aConfig.GetObjectEntries

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

                    If aConfigmember.IsXChanged Then
                        If IsNumeric(aConfigmember.Ordinal.Value) Then
                            i = CLng(aConfigmember.Ordinal.Value)
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
                'Call aConfig.RunXChange(aMapping:=aMapping)

                ' save the row
                i = 0
                For Each aConfigmember In listofAttributes
                    If aConfigmember.IsXChanged Then
                        If IsNumeric(aConfigmember.Ordinal.Value) Then
                            i = CLng(aConfigmember.Ordinal.Value)
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