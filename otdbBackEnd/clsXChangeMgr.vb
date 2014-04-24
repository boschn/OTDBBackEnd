
REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** XChangeManager Classes Runtime Structures 
REM ***********
REM *********** Version: X.YY
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************
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
        Private _datatype As otDataType = 0

        ' result
        Private _convertSucceeded As Boolean = False
        Private _msglog As ObjectLog

        Public Sub New(datatype As otDataType, valuetype As convertValueType, value As Object,
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
        Public Property Datatype() As otDataType
            Get
                Return Me._datatype
            End Get
            Set(value As otDataType)
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
    ''' XBag is an arbitary XChange Data Object which constists of different XEnvelopes ordered by
    ''' ordinals.
    ''' An XBag an Default persistable XChangeConfig
    ''' </summary>
    ''' <remarks></remarks>
    Public Class XBag
        Implements IEnumerable(Of XBag)

        '* default Config we are looking over
        Private _XChangeDefaultConfig As XChangeConfiguration
        Private _XCmd As otXChangeCommandType = 0

        '* real Attributes used after prepared
        Private _usedAttributes As New Dictionary(Of String, IXChangeConfigEntry)
        Private _usedObjects As New Dictionary(Of String, IXChangeConfigEntry)

        '** all the member envelopes
        Private WithEvents _defaultEnvelope As New XEnvelope(Me)
        Private WithEvents _envelopes As New SortedDictionary(Of Ordinal, XEnvelope)

        '** flags

        Private _isPrepared As Boolean = False

        Private _PreparedOn As Date = constNullDate

        Private _IsPrechecked As Boolean = False
        Private _PrecheckedOk As Boolean = False
        Private _PrecheckTimestamp As Date = constNullDate

        Private _isProcessed As Boolean = False
        Private _XChangedOK As Boolean = False
        Private _ProcessedTimestamp As Date = constNullDate


        '** events for convert values
        Public Event ConvertRequest2HostValue As EventHandler(Of ConvertRequestEventArgs)
        Public Event ConvertRequest2DBValue As EventHandler(Of ConvertRequestEventArgs)


        Public Sub New(xchangeDefaultConfig As XChangeConfiguration)
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
        Public ReadOnly Property XChangeDefaultConfig() As XChangeConfiguration
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
                RunXChange = RunXChange And anEnvelope.RunXChange
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
        Private _xattribute As XChangeObjectEntry
        Private _explicitDatatype As otDataType

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
        Public Sub New(xenvelope As XEnvelope, attribute As XChangeObjectEntry)
            _envelope = xenvelope
            _xattribute = attribute
            _ordinal = attribute.Ordinal
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
        Public Sub New(xenvelope As XEnvelope, attribute As XChangeObjectEntry, hostvalue As Object, Optional isEmpty As Boolean = False, Optional isNull As Boolean = False)
            _envelope = xenvelope
            _xattribute = attribute
            _ordinal = attribute.Ordinal
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

        Public Property Datatype As otDataType
            Get
                If _xattribute IsNot Nothing And _explicitDatatype = 0 Then
                    Return _xattribute.[ObjectEntryDefinition].Datatype
                ElseIf _explicitDatatype <> 0 Then
                    Return _explicitDatatype
                Else
                    CoreMessageHandler(message:="Attribute or Datatype not set in slot", messagetype:=otCoreMessageType.InternalError, subname:="XSlot.Datatype")
                    Return 0
                End If
            End Get
            Set(value As otDataType)
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
        Public Property XAttribute() As XChangeObjectEntry
            Get
                Return Me._xattribute
            End Get
            Set(value As XChangeObjectEntry)
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

        Public Shared Function DefaultConvert2HostValue(ByRef datatype As otDataType,
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
                Case otDataType.[Long]
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
                        Call CoreMessageHandler(subname:="IXChangeConfigEntry.convertValue2Hostvalue",
                                              message:="OTDB data " & dbvalue & " is not convertible to long",
                                              arg1:=dbvalue)
                        hostValueIsEmpty = True
                        Return False
                    End If
                Case otDataType.Numeric
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
                        Call CoreMessageHandler(subname:="IXChangeConfigEntry.convertValue2Hostvalue",
                                              message:="OTDB data " & dbvalue & " is not convertible to double",
                                              arg1:=dbvalue)
                        hostvalue = Nothing
                        hostValueIsEmpty = True
                        Return False
                    End If


                Case otDataType.Text, otDataType.List, otDataType.Memo

                    hostvalue = CStr(dbvalue)
                    Return True

                Case otDataType.Runtime
                    Call CoreMessageHandler(subname:="IXChangeConfigEntry.convertValue2Hostvalue",
                                            message:="OTDB data " & dbvalue & " is not convertible to runtime",
                                            arg1:=dbvalue)
                    hostvalue = Nothing
                    hostValueIsEmpty = True
                    Return False

                Case otDataType.Formula
                    Call CoreMessageHandler(subname:="IXChangeConfigEntry.convertValue2Hostvalue",
                                            message:="OTDB data " & dbvalue & " is not convertible to formula",
                                            arg1:=dbvalue)
                    hostvalue = Nothing
                    hostValueIsEmpty = True
                    Return False

                Case otDataType.[Date], otDataType.Time, otDataType.Timestamp
                    If dbValueIsNull OrElse IsDBNull(dbvalue) OrElse dbvalue = constNullDate OrElse dbvalue = ConstNullTime Then
                        If datatype = otDataType.Time Then
                            hostvalue = ConstNullTime ' HACK ! Should be Default Null Value
                        Else
                            hostvalue = constNullDate
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
                        Call CoreMessageHandler(subname:="IXChangeConfigEntry.convertValue2Hostvalue",
                                              message:="OTDB data " & dbvalue & " is not convertible to date, time, timestamp",
                                              arg1:=dbvalue)
                        hostvalue = Nothing
                        hostValueIsEmpty = True
                        Return False
                    End If

                Case otDataType.Bool
                    hostvalue = dbvalue
                    Return True
                Case otDataType.Binary
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
        Public Shared Function DefaultConvert2DBValue(ByRef datatype As otDataType,
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

                Case otDataType.Numeric, otDataType.[Long]
                    If hostvalue Is Nothing OrElse hostValueIsNull Then
                        dbvalue = DBNull.Value
                        dbValueIsNull = True
                        Return True
                    ElseIf IsNumeric(hostvalue) Then
                        If datatype = otDataType.Numeric Then
                            dbvalue = CDbl(hostvalue)    ' simply keep it
                            Return True
                        Else
                            dbvalue = CLng(hostvalue)
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


                Case otDataType.Text, otDataType.List, otDataType.Memo

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

                Case otDataType.Runtime
                    Call CoreMessageHandler(subname:="XSlot.convert2DBValue",
                                          message:="OTDB data " & hostvalue & " is not convertible from/to runtime",
                                           arg1:=hostvalue)

                    dbvalue = DBNull.Value
                    Return False

                Case otDataType.Formula
                    Call CoreMessageHandler(subname:="XSlot.convert2DBValue", arg1:=hostvalue.ToString,
                                          message:="OTDB data " & hostvalue & " is not convertible from/to formula")

                    dbvalue = Nothing
                    dbValueIsEmpty = True
                    Return False

                Case otDataType.[Date], otDataType.Time, otDataType.Timestamp
                    If hostvalue Is Nothing OrElse hostValueIsNull = True Then
                        dbvalue = constNullDate
                        dbValueIsNull = True
                        Return True
                    ElseIf IsDate(hostvalue) Then
                        dbvalue = CDate(hostvalue)
                        Return True
                    Else
                        Call CoreMessageHandler(subname:="XSlot.convert2DBValue",
                                              message:="OTDB data " & hostvalue & " is not convertible to Date",
                                              arg1:=hostvalue)

                        dbvalue = constNullDate
                        dbValueIsEmpty = True
                        Return False
                    End If

                Case otDataType.Bool
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

                Case otDataType.Binary
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

    ''' <summary>
    ''' XChange Envelope is a Member of a Bag and Contains Pairs of ordinal, XSlot
    ''' </summary>
    ''' <remarks></remarks>
    Public Class XEnvelope
        Implements IEnumerable(Of XSlot)

        Private _xbag As XBag
        Private _xchangeconfig As XChangeConfiguration

        Private _IsPrechecked As Boolean = False
        Private _PrecheckedOk As Boolean = False
        Private _PrecheckTimestamp As Date = constNullDate

        Private _IsXChanged As Boolean = False
        Private _XChangedOK As Boolean = False
        Private _XChangedTimestamp As Date = constNullDate

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
        Public ReadOnly Property Xchangeconfig() As XChangeConfiguration
            Get
                Return Me._xchangeconfig
            End Get
        End Property
#End Region

#Region "Administrative Function"


        Public ReadOnly Property Ordinals() As System.Collections.Generic.SortedDictionary(Of Ordinal, XSlot).KeyCollection
            Get
                Return _slots.Keys
            End Get
        End Property

        '**** check functions if exists
        Public Function ContainsOrdinal(ByVal [ordinal] As Ordinal) As Boolean
            Return _slots.ContainsKey(ordinal)
        End Function
        Public Function ContainsOrdinal(ByVal [ordinal] As Long) As Boolean
            Return Me.ContainsOrdinal(New Ordinal([ordinal]))
        End Function
        Public Function ContainsOrdinal(ByVal [ordinal] As String) As Boolean
            Return Me.ContainsOrdinal(New Ordinal([ordinal]))
        End Function
        ''' <summary>
        ''' returns true if in the XConfig a Slot is available for the entryname
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasConfigObjectEntryname(ByVal entryname As String, Optional objectname As String = "") As Boolean
            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.HasConfigObjectEntryname")
                Return False
            End If

            Dim aXChangeMember = _xchangeconfig.GetEntryByObjectEntryName(entryname:=entryname, objectname:=objectname)

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
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasConfigXID(ByVal xid As String, Optional objectname As String = "") As Boolean

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.getvaluebyID")
                Return Nothing
            End If

            Dim anEntry = _xchangeconfig.GetEntryByXID(XID:=xid, objectname:=objectname)
            If anEntry Is Nothing Then
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
            If Me.ContainsOrdinal(ordinal) Then
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
            If Not Me.ContainsOrdinal(ordinal) Then
                Dim theEntryList = Me.Xchangeconfig.GetEntriesByMappingOrdinal(ordinal:=ordinal)
                Dim anAttribute As XChangeObjectEntry = Nothing
                For Each anEntry In theEntryList
                    If anEntry.IsObjectEntry Then
                        anAttribute = TryCast(anEntry, XChangeObjectEntry)
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
            If Me.ContainsOrdinal(ordinal) Then
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
        ''' returns a Slot by mapping ordinal
        ''' </summary>
        ''' <param name="ordinal"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSlot(ByRef ordinal As Ordinal) As XSlot
            If Me.ContainsOrdinal(ordinal) Then
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
            For Each anAttribute In Me.Xchangeconfig.GetEntriesByObjectName(objectname:=objectname)
                If Me.HasSlotByObjectEntryName(entryname:=anAttribute.ObjectEntryname, objectname:=objectname) Then
                    aList.Add(Me.GetSlot(ordinal:=anAttribute.Ordinal))
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
            If Me.ContainsOrdinal(slot.ordinal) Then
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
        ''' <param name="objectname"></param>
        ''' <param name="replaceSlotIfExists"></param>
        '''  <param name="extendXConfig"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddSlotByXID(ByVal xid As String, ByVal value As Object,
                                    Optional ByVal isHostValue As Boolean = True,
                                    Optional objectname As String = "",
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

            Dim anEntry = _xchangeconfig.GetEntryByXID(XID:=xid, objectname:=objectname)
            Return Me.AddSlotbyXEntry(entry:=anEntry, value:=value, isHostValue:=isHostValue, SlotIsEmpty:=SlotIsEmpty, ValueIsNull:=ValueIsNull, _
                                      replaceSlotIfexists:=replaceSlotIfExists)

            If anEntry Is Nothing And extendXConfig Then
                _xchangeconfig.AddEntryByXID(Xid:=xid, objectname:=objectname, [readonly]:=isReadonly, isXChanged:=isXchanged, xcmd:=xcmd)
                anEntry = _xchangeconfig.GetEntryByXID(XID:=xid, objectname:=objectname)
            End If

            If anEntry IsNot Nothing Then
                Return Me.AddSlotbyXEntry(entry:=anEntry, value:=value, isHostValue:=isHostValue, SlotIsEmpty:=SlotIsEmpty, ValueIsNull:=ValueIsNull, _
                                      replaceSlotIfexists:=replaceSlotIfExists)
            Else
                Return False
            End If
        End Function
        ''' <summary>
        ''' Add a Slot by entryname
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <param name="value"></param>
        ''' <param name="isHostValue"></param>
        ''' <param name="objectname"></param>
        ''' <param name="overwriteValue"></param>
        ''' <param name="extendXConfig"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddSlotByObjectEntryName(ByVal entryname As String, ByVal value As Object,
                                           Optional ByVal isHostValue As Boolean = True,
                                            Optional objectname As String = "",
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

            Dim anEntry = _xchangeconfig.GetEntryByObjectEntryName(entryname:=entryname, objectname:=objectname)
            If anEntry Is Nothing And extendXConfig Then
                _xchangeconfig.AddEntryByObjectEntry(entryname:=entryname, objectname:=objectname, isXChanged:=isXchanged, [readonly]:=isReadonly, _
                                                   xcmd:=xcmd)
                anEntry = _xchangeconfig.GetEntryByObjectEntryName(entryname:=entryname, objectname:=objectname)
            End If

            If anEntry IsNot Nothing Then
                Return Me.AddSlotbyXEntry(entry:=anEntry, value:=value, isHostValue:=isHostValue, overwriteValue:=overwriteValue, _
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
        ''' <param name="objectname"></param>
        ''' <param name="overwriteValue"></param>
        ''' <param name="removeSlotIfExists"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddSlotbyXEntry(ByRef entry As IXChangeConfigEntry, ByVal value As Object,
                                        Optional ByVal isHostValue As Boolean = True,
                                        Optional objectname As String = "",
                                        Optional overwriteValue As Boolean = False,
                                        Optional replaceSlotIfexists As Boolean = False, _
                                        Optional ValueIsNull As Boolean = False, _
                                        Optional SlotIsEmpty As Boolean = False) As Boolean

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.AddSlotbyXEntry")
                Return False
            End If

            If Not entry Is Nothing AndAlso (entry.IsLoaded Or entry.IsCreated) Then
                If Me.ContainsOrdinal(ordinal:=entry.Ordinal) And Not replaceSlotIfexists Then
                    If overwriteValue Then
                        Dim aSlot As XSlot = _slots.Item(key:=entry.Ordinal)
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
                    Dim aNewSlot As XSlot = New XSlot(Me, attribute:=entry)
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
        ''' <param name="objectname"></param>
        ''' <param name="asHostValue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSlotValueByXID(ByVal xid As String, Optional objectname As String = "", Optional asHostValue As Boolean = True) As Object

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.GetSlotValueByXID")
                Return Nothing
            End If

            Dim aXChangeMember = _xchangeconfig.GetEntryByXID(XID:=xid, objectname:=objectname)
            If aXChangeMember IsNot Nothing Then
                Return Me.GetSlotValueByXEntry(aXChangeMember)
            Else
                CoreMessageHandler(message:="XChangeConfig '" & Me.Xchangeconfig.Configname & "' does not include the id", arg1:=xid, messagetype:=otCoreMessageType.ApplicationWarning, subname:="XEnvelope.GetSlotValueByID")
                Return Nothing
            End If

        End Function
        ''' <summary>
        ''' return true if there is a slot by ID
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasSlotByXID(ByVal xid As String, Optional objectname As String = "") As Object

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.HasSlotByXID")
                Return Nothing
            End If

            Dim aXChangeMember = _xchangeconfig.GetEntryByXID(XID:=xid, objectname:=objectname)
            If aXChangeMember IsNot Nothing Then
                Return Me.HasSlotByXEntry(aXChangeMember)
            Else
                Return False
            End If

        End Function
        ''' <summary>
        ''' returns the slot's value by entryname
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <param name="objectname"></param>
        ''' <param name="asHostValue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSlotValueByObjectEntryName(ByVal entryname As String, Optional objectname As String = "", Optional asHostValue As Boolean = True) As Object

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.GetSlotValueByObjectEntryName")
                Return Nothing
            End If

            Dim aXChangeMember As XChangeObjectEntry = _xchangeconfig.GetEntryByObjectEntryName(entryname:=entryname, objectname:=objectname)
            If aXChangeMember IsNot Nothing Then
                Return Me.GetSlotValueByXEntry(aXChangeMember)
            Else
                CoreMessageHandler(message:="xconfiguration '" & Me.Xchangeconfig.Configname & "' does not include entryname", entryname:=entryname, objectname:=objectname, _
                                   messagetype:=otCoreMessageType.ApplicationWarning, subname:="Xenvelope.GetSlotValueByObjectEntryName")
                Return Nothing
            End If

        End Function
        ''' <summary>
        ''' returns true if there is a slot by entryname
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasSlotByObjectEntryName(ByVal entryname As String, Optional objectname As String = "") As Boolean

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.HasSlotByObjectEntryName")
                Return Nothing
            End If
            Dim aXChangeMember As XChangeObjectEntry = _xchangeconfig.GetEntryByObjectEntryName(entryname:=entryname, objectname:=objectname)
            If aXChangeMember IsNot Nothing Then
                Return Me.HasSlotByXEntry(aXChangeMember)
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
        Public Function GetSlotValueByXEntry(ByRef entry As XChangeObjectEntry, Optional asHostValue As Boolean = True) As Object

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.GetSlotValueByXEntry")
                Return Nothing
            End If

            If Not entry Is Nothing AndAlso (entry.IsLoaded Or entry.IsCreated) Then
                Return Me.GetSlotValue(ordinal:=New Ordinal(entry.Ordinal), asHostvalue:=asHostValue)
            Else
                Call CoreMessageHandler(message:="entry is nothing", messagetype:=otCoreMessageType.InternalWarning, subname:="XEnvelope.GetSlotValueByEntry")
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns True if there is a slot by XConfig Member by XChangemember
        ''' </summary>
        ''' <param name="xchangemember"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasSlotByXEntry(ByRef objectentry As XChangeObjectEntry) As Boolean

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.HasSlotByXEntry")
                Return False
            End If

            If objectentry IsNot Nothing AndAlso (objectentry.IsLoaded Or objectentry.IsCreated) Then
                If _slots.ContainsKey(key:=objectentry.Ordinal) Then
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If
        End Function
        ''' <summary>
        ''' returns the Attribute of a slot by entryname and objectname
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntryByObjectEntryname(ByVal entryname As String, Optional objectname As String = "") As XChangeObjectEntry

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.GetEntryByObjectEntryname")
                Return Nothing
            End If

            Return _xchangeconfig.GetEntryByObjectEntryName(entryname:=entryname, objectname:=objectname)
        End Function
        ''' <summary>
        ''' returns the Entry of a slot by xid and objectname
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntryByXID(ByVal XID As String, Optional objectname As String = "") As XChangeObjectEntry

            If _xchangeconfig Is Nothing Then
                Call CoreMessageHandler(message:="XChangeConfig is not set within XMAP", subname:="XEnvelope.GetEntryByXID")
                Return Nothing
            End If

            Return _xchangeconfig.GetEntryByXID(XID:=XID, objectname:=objectname)
        End Function

        ''' <summary>
        ''' returns the Slotsvalue as hostvalue or dbvalue by ordinal key
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="asHostvalue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSlotValue(ByVal key As Long, Optional ByVal asHostValue As Boolean = False) As Object
            Return Me.GetSlotValue(ordinal:=New Ordinal(key), asHostvalue:=asHostValue)
        End Function
        ''' <summary>
        ''' returns the Slotsvalue as hostvalue or dbvalue by ordinal key
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
            Dim anObject As XChangeObject = Me.Xchangeconfig.GetObjectByName(objectname:=objectname)
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
                Select Case anObject.Objectname.ToUpper

                    ' currtargets
                    Case Deliverables.CurrentTarget.ConstObjectID.ToUpper
                        flag = True

                        ' currschedules
                    Case Scheduling.CurrentSchedule.ConstObjectID.ToUpper
                        flag = True

                        ' schedules
                    Case Scheduling.Schedule.ConstObjectID.ToUpper
                        flag = True

                        ' Targets
                    Case Deliverables.Target.ConstObjectID.ToUpper
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

            If _XChangedTimestamp = constNullDate Then
                _XChangedTimestamp = Date.Now
            End If

            '* go through each object
            For Each anConfigObject As XChangeObject In Me.Xchangeconfig.ObjectsByOrderNo
                flag = False
                ' special handling for special objects
                Select Case anConfigObject.Objectname.ToLower

                    ' currschedules
                    Case CurrentSchedule.ConstObjectID.ToLower
                        flag = True

                    Case XOutline.constobjectid.ToLower
                        flag = True

                        ' Tracks
                    Case Track.ConstObjectID.ToLower
                        flag = True

                        ' HACK: CARTYPES
                    Case "tblconfigs"
                        'flag = flag And aDeliverable.runCartypesXChange(Me, msglog)
                        flag = True

                        ' Targets
                    Case Target.ConstObjectID.ToLower
                        'flag = flag And aTarget.runXChange(Me, msglog)
                        flag = True
                End Select

                '****
                '**** Standards
                If Not flag Then
                    '** check through reflection
                    Dim anObjectType As System.Type = ot.GetObjectClassType(anConfigObject.Objectname)
                    If anObjectType IsNot Nothing AndAlso _
                        anObjectType.GetInterface(GetType(iotXChangeable).FullName) IsNot Nothing Then

                        Dim aXChangeable As iotXChangeable = Activator.CreateInstance(anObjectType)
                        'flag = flag And aXChangeable.RunXChange(Me)
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
        'Private Function CreateandUpdateObject(ByRef xobject As XChangeObject,
        '                                       ByRef record As ormRecord,
        '                                       ByRef pkarray() As Object,
        '                                       Optional ByRef msglog As ObjectLog = Nothing
        '                                        ) As Boolean
        '    Dim aDataObjectType As System.Type
        '    Dim aDataobject As iormPersistable
        '    Dim aDataInfusable As iormInfusable
        '    Dim aValue As Object = Nothing
        '    Dim anOldValue As Object = Nothing
        '    Dim aSlot As XSlot

        '    Dim persistflag As Boolean = False

        '    '** BETTER TO CREATE A NEW OBJECT -> Default values
        '    If ot.GetObjectClassType(objectname:=xobject.Objectname) IsNot Nothing Then
        '        aDataObjectType = ot.GetObjectClassType(objectname:=xobject.Objectname)
        '        aDataobject = Activator.CreateInstance(aDataObjectType)
        '        aDataInfusable = aDataobject
        '    Else
        '        aDataobject = Nothing
        '        aDataInfusable = Nothing
        '    End If

        '    '** create new object
        '    '**
        '    If xobject.XChangeCmd = otXChangeCommandType.Update And record Is Nothing Then
        '        Return False
        '    ElseIf xobject.XChangeCmd = otXChangeCommandType.UpdateCreate And record Is Nothing Then
        '        '** RECORD based Object creation
        '        record = New ormRecord
        '        record.SetTable(xobject.Objectname, fillDefaultValues:=True)

        '        '** BETTER TO CREATE A NEW OBJECT -> Default values
        '        If aDataobject IsNot Nothing Then
        '            If Not aDataobject.Create(pkArray:=pkarray) Then
        '                CoreMessageHandler(message:="Data object with same primary keys exists", messagetype:=otCoreMessageType.ApplicationError, subname:="XEnvelope.RunDefaultXChange4Object")
        '                aDataobject.Inject(pkArray:=pkarray)
        '            End If
        '        End If
        '        '** set to updateCreate
        '        For Each anAttribute In Me.Xchangeconfig.GetEntriesByObjectName(objectname:=xobject.Objectname)
        '            anAttribute.XChangeCmd = otXChangeCommandType.UpdateCreate
        '            anAttribute.IsXChanged = True
        '        Next
        '    End If

        '    '*** set values of object
        '    '***
        '    For Each anAttribute In Me.Xchangeconfig.GetEntriesByObjectName(objectname:=xobject.Objectname)
        '        If anAttribute.IsXChanged AndAlso Not anAttribute.IsReadOnly Then
        '            If (anAttribute.XChangeCmd = otXChangeCommandType.Update Or anAttribute.XChangeCmd = otXChangeCommandType.UpdateCreate) Then
        '                aSlot = Me.GetSlot(ordinal:=anAttribute.Ordinal)
        '                If aSlot IsNot Nothing AndAlso Not aSlot.IsEmpty Then
        '                    '* get Value from Slot
        '                    aValue = aSlot.DBValue
        '                    '* get old value
        '                    If record.HasIndex(index:=anAttribute.ObjectEntryname) Then
        '                        anOldValue = record.GetValue(index:=anAttribute.ObjectEntryname)
        '                    Else
        '                        anOldValue = Nothing
        '                    End If
        '                    '** change if different and not empty
        '                    If aValue <> anOldValue Then
        '                        record.SetValue(index:=anAttribute.ObjectEntryname, value:=aValue)
        '                        persistflag = True
        '                    End If
        '                End If
        '            End If
        '        End If
        '    Next

        '    '' if a new record has not all fields set -> ?! what to do then ?

        '    '** BETTER TO CREATE A NEW OBJECT -> Default values
        '    If persistflag Then
        '        If aDataobject IsNot Nothing Then
        '            aDataInfusable.Infuse(record)
        '            Return aDataobject.Persist()
        '        Else
        '            Return record.Persist
        '        End If
        '    End If

        'End Function

        ''' <summary>
        ''' Run the default xchange for a given record
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="dataobject"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        'Public Function RunDefaultXchange(ByRef record As ormRecord, _
        '                                  Optional xobject As XChangeObject = Nothing, _
        '                                  Optional pkArray As Object() = Nothing, _
        '                                  Optional ByRef msglog As ObjectLog = Nothing,
        '                                  Optional ByVal nocompounds As Boolean = False) As Boolean
        '    Dim aValue As Object


        '    '* get the config
        '    If xobject Is Nothing Then
        '        'xobject = Me.Xchangeconfig.ObjectByName(objectname:=record.TableIDs)
        '    End If

        '    ' set msglog
        '    If msglog Is Nothing Then
        '        If _msglog Is Nothing Then
        '            _msglog = New ObjectLog
        '        End If
        '        msglog = _msglog
        '        'msglog.Create(Me.msglogtag)
        '    End If

        '    '** no record is given
        '    If record IsNot Nothing Then
        '        '*** load the record fields not the compounds !
        '        '***
        '        For Each aFieldname In record.Keys
        '            Dim anObjectEntry = CurrentSession.Objects.GetEntry(entryname:=aFieldname, objectname:=xobject.Objectname)

        '            If Me.HasConfigObjectEntryname(entryname:=aFieldname, objectname:=xobject.Objectname) AndAlso Not anObjectEntry.IsCompound Then
        '                '* get the value and add it -> will be replaced as well !
        '                aValue = record.GetValue(aFieldname)
        '                If aValue IsNot Nothing Then
        '                    Me.AddSlotByObjectEntryName(entryname:=aFieldname, objectname:=xobject.Objectname, value:=aValue, isHostValue:=False,
        '                                          overwriteValue:=False, extendXConfig:=False)
        '                End If
        '            End If
        '        Next

        '        '*** load the compounds
        '        '***
        '        If Not nocompounds Then
        '            Dim objectType As System.Type = ot.GetObjectClassType(objectname:=xobject.Objectname)
        '            If objectType IsNot Nothing AndAlso objectType.GetInterface(GetType(iotHasCompounds).FullName) IsNot Nothing Then
        '                Dim aHasCompounds As iotHasCompounds = Activator.CreateInstance(objectType)
        '                Dim aInfusable As iormInfusable = TryCast(aHasCompounds, iormInfusable)
        '                If aInfusable IsNot Nothing Then
        '                    aInfusable.Infuse(record)
        '                    If aHasCompounds IsNot Nothing Then
        '                        aHasCompounds.AddSlotCompounds(Me)
        '                    Else
        '                        CoreMessageHandler(message:="the object of type " & xobject.Objectname & " cannot be casted to hasCompunds", _
        '                                           subname:="XEnvelope.RunDefaultxChange", messagetype:=otCoreMessageType.InternalError)
        '                    End If
        '                Else
        '                    CoreMessageHandler(message:="the object of type " & xobject.Objectname & " cannot be infused", _
        '                                           subname:="XEnvelope.RunDefaultxChange", messagetype:=otCoreMessageType.InternalError)

        '                End If

        '            End If
        '        End If

        '    End If

        '    '*** run the command
        '    '***
        '    Select Case xobject.XChangeCmd


        '        '*** delete
        '        '***
        '        Case otXChangeCommandType.Delete

        '            '**** add or update
        '            '****
        '        Case otXChangeCommandType.Update, otXChangeCommandType.UpdateCreate
        '            '* if no primary keys then refill it with the object definition from the record
        '            If pkArray Is Nothing Then
        '                ReDim pkArray(xobject.ObjectDefinition.GetNoKeys)
        '                '**** fill the primary key structure
        '                Dim i As UShort = 0
        '                For Each aPKEntry In xobject.ObjectDefinition.GetKeyEntries
        '                    aValue = record.GetValue(index:=aPKEntry.Entryname)
        '                    If aValue Is Nothing Then
        '                        '* try to load from Envelope if not in record
        '                        aValue = Me.GetSlotValueByObjectEntryName(entryname:=aPKEntry.Entryname, objectname:=xobject.Objectname, asHostValue:=False)
        '                        If aValue IsNot Nothing Then
        '                            record.SetValue(index:=aPKEntry.Entryname, value:=aValue) ' set it also in the record
        '                        End If
        '                    End If
        '                    If aValue IsNot Nothing Then
        '                        '** convert from DB to Host
        '                        pkArray(i) = aValue
        '                        i += 1
        '                    Else
        '                        Call CoreMessageHandler(message:="value of primary key is not in configuration or envelope :" & xobject.Configname,
        '                                         arg1:=xobject.Objectname, entryname:=aPKEntry.Entryname, messagetype:=otCoreMessageType.ApplicationError,
        '                                         subname:="XEnvelope.runDefaultXChange(Record)")
        '                        Return False
        '                    End If

        '                Next
        '            End If
        '            '** create and Update the object
        '            Return Me.CreateandUpdateObject(xobject:=xobject, record:=record, pkarray:=pkArray)

        '            '*** duplicate
        '            '***
        '        Case otXChangeCommandType.Duplicate

        '            '***
        '            '*** just read and return
        '        Case otXChangeCommandType.Read
        '            Return Not record Is Nothing

        '            '**** no command ?!
        '        Case Else
        '            Call CoreMessageHandler(message:="XChangeCmd for this object is not known :" & xobject.Objectname,
        '                              arg1:=xobject.XChangeCmd, objectname:=xobject.Objectname, messagetype:=otCoreMessageType.ApplicationError,
        '                              subname:="XEnvelope.runDefaultXChange(Record)")
        '            Return False
        '    End Select



        'End Function

        ''' <summary>
        ''' Run the default xchange on a given and alive dataobject
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="dataobject"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunXChangeCommand(ByRef dataobject As iormPersistable, type As System.Type, _
                                          Optional xobject As XChangeObject = Nothing, _
                                          Optional ByRef msglog As ObjectLog = Nothing) As Boolean
            Dim aValue As Object

            '* get the config
            If xobject Is Nothing Then
                xobject = Me.Xchangeconfig.GetObjectByName(objectname:=dataobject.ObjectID)
            End If

            ' set msglog
            If msglog Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                msglog = _msglog
                'msglog.Create(Me.msglogtag)
            End If

            '** no object is given
            If dataobject Is Nothing OrElse Not dataobject.isAlive(throwError:=False) Then
                CoreMessageHandler(message:="dataobject must be alive to xchange it from an Envelope", subname:="XEnvelope.RunXChangeCMD", _
                                   messagetype:=otCoreMessageType.InternalError, objectname:=dataobject.ObjectID)
                Return False
            End If

            '** run the read first -> to fill the envelope anyway what is happening afterwards
            '** 

            For Each anObjectEntry In dataobject.ObjectDefinition.GetEntries()
                If Me.HasConfigObjectEntryname(entryname:=anObjectEntry.Entryname, objectname:=xobject.Objectname) Then
                    '* get the value and add it -> will be replaced as well !
                    aValue = dataobject.GetValue(anObjectEntry.Entryname)
                    ' add it to the slot even if it's nothing -> default must be converted through the
                    ' slot
                    ' add the slot but donot extend the XConfig - donot overwrite existing values
                    Me.AddSlotByObjectEntryName(entryname:=anObjectEntry.Entryname, _
                                                objectname:=xobject.Objectname, _
                                                value:=aValue, _
                                                isHostValue:=False,
                                                overwriteValue:=False, _
                                                extendXConfig:=False)

                End If
            Next


            '*** run the commands
            '***
            Select Case xobject.XChangeCmd


                '*** delete
                '***
                Case otXChangeCommandType.Delete
                    Return dataobject.Delete()

                    '**** add or update
                    '****
                Case otXChangeCommandType.Update, otXChangeCommandType.UpdateCreate
                    '*** set values of object
                    '***
                    Dim persistflag As Boolean = False
                    For Each anXEntry In Me.Xchangeconfig.GetEntriesByObjectName(objectname:=xobject.Objectname)
                        If anXEntry.IsXChanged AndAlso Not anXEntry.IsReadOnly Then
                            If (anXEntry.XChangeCmd = otXChangeCommandType.Update Or anXEntry.XChangeCmd = otXChangeCommandType.UpdateCreate) Then
                                Dim aSlot = Me.GetSlot(ordinal:=anXEntry.Ordinal)
                                If aSlot IsNot Nothing AndAlso Not aSlot.IsEmpty Then
                                    '* get Value from Slot
                                    aValue = aSlot.DBValue
                                    If dataobject.ObjectDefinition.HasEntry(anXEntry.ObjectEntryname) Then
                                        persistflag = persistflag Or dataobject.SetValue(entryname:=anXEntry.ObjectEntryname, value:=aValue)
                                    End If
                                End If
                            End If
                        End If
                    Next
                    If persistflag Then
                        Return dataobject.Persist()
                    Else
                        Return True ' even if not persisted the operation is successfull
                    End If


                    '*** duplicate
                    '***
                Case otXChangeCommandType.Duplicate
                    'dataobject.clone().persist
                    Throw New NotImplementedException
                    '***
                    '*** just read and return
                Case otXChangeCommandType.Read
                    '** the xenvelope was already filled with data
                    '** just return successfull
                    Return True

                    '**** no command ?!
                Case Else
                    Call CoreMessageHandler(message:="XChangeCmd for this object is not known :" & xobject.Objectname,
                                      arg1:=xobject.XChangeCmd, objectname:=xobject.Objectname, messagetype:=otCoreMessageType.ApplicationError,
                                      subname:="XEnvelope.runXChangeCMD")
                    Return False
            End Select


        End Function

        ''' <summary>
        ''' Run the Default XChange for an object by primary keys
        ''' </summary>
        ''' <param name="xobject"></param>
        ''' <param name="msglog"></param>
        ''' <param name="nocompounds"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunDefaultXChange(ByRef xobject As XChangeObject,
                                          Optional ByRef msglog As ObjectLog = Nothing) As Boolean
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
            If xobject.ObjectDefinition.GetNoKeys = 0 Then
                Call CoreMessageHandler(message:="primary key of table is Nothing in xchange config:" & xobject.Configname,
                                      arg1:=xobject.Objectname, messagetype:=otCoreMessageType.InternalError, subname:="XEnvelope.runDefaultXChange4Object")
                Return False
            Else
                ReDim pkarry(xobject.ObjectDefinition.GetNoKeys - 1)
            End If

            '**** fill the primary key structure
            Dim i As UShort = 0
            For Each aPKEntry In xobject.ObjectDefinition.GetKeyEntries
                aValue = Me.GetSlotValueByObjectEntryName(entryname:=aPKEntry.Entryname, objectname:=aPKEntry.Objectname, asHostValue:=False)
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

            ''' check if we need a object and how to get it
            ''' then run the command

            Dim anObject As iormPersistable
            Select Case xobject.XChangeCmd
                Case otXChangeCommandType.UpdateCreate
                    anObject = ormDataObject.Retrieve(pkarry, xobject.ObjectDefinition.ObjectType)
                    If anObject Is Nothing Then anObject = ormDataObject.CreateDataObject(pkarry, xobject.ObjectDefinition.ObjectType, domainID:=CurrentSession.CurrentDomainID)
                Case otXChangeCommandType.Delete, otXChangeCommandType.Duplicate, otXChangeCommandType.Read, otXChangeCommandType.Update
                    '*** read the data
                    '***
                    anObject = ormDataObject.Retrieve(pkarry, xobject.ObjectDefinition.ObjectType)
                Case Else
                    CoreMessageHandler(message:="XCMD is not implemented for XConfig " & xobject.Configname, subname:="Xenvelope.RunDefaultXChange", arg1:=xobject.XChangeCmd, messagetype:=otCoreMessageType.InternalError)
                    Return False
            End Select

            '** run it with the object
            If anObject IsNot Nothing Then
                Return Me.RunXChangeCommand(anObject, type:=xobject.ObjectDefinition.ObjectType, xobject:=xobject, msglog:=msglog)
            Else
                CoreMessageHandler(message:="OnTrack DataObject couldnot be retrieved nor created: " & xobject.Objectname, subname:="Xenvelope.RunDefaultXChange", arg1:=xobject.XChangeCmd, messagetype:=otCoreMessageType.InternalError)

                Return False
            End If

        End Function

        '***** fillMappingWithCompounds
        '*****
        Private Function fillMappingWithCompounds(ByRef RECORD As ormRecord, ByRef MAPPING As Dictionary(Of Object, Object),
                                                  ByRef ORIGMAPPING As Dictionary(Of Object, Object),
        ByRef TABLE As ObjectDefinition,
        Optional ByRef MSGLOG As ObjectLog = Nothing) As Boolean
            Dim aConfigmember As IXChangeConfigEntry
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim aCompRecordColl As New List(Of ormRecord)
            Dim aCompTableDir As New Dictionary(Of String, Dictionary(Of String, iormObjectEntry))
            Dim compoundKeys As Object
            Dim aCompField As Object
            Dim aCompValue As Object
            Dim objectname As Object
            Dim anObjectEntry As iormObjectEntry
            Dim anEntryDir As New Dictionary(Of String, iormObjectEntry)
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
            For Each m In TABLE.GetEntries
                anObjectEntry = m
                If anObjectEntry.XID <> "" And anObjectEntry.IsCompound Then
                    If aCompTableDir.ContainsKey(key:=DirectCast(anObjectEntry, ObjectCompoundEntry).CompoundTablename) Then
                        anEntryDir = aCompTableDir.Item(key:=DirectCast(anObjectEntry, ObjectCompoundEntry).CompoundTablename)
                    Else
                        anEntryDir = New Dictionary(Of String, iormObjectEntry)
                        Call aCompTableDir.Add(key:=DirectCast(anObjectEntry, ObjectCompoundEntry).CompoundTablename, value:=anEntryDir)
                    End If
                    ' add the Entry
                    If Not anEntryDir.ContainsKey(key:=anObjectEntry.XID) Then
                        Call anEntryDir.Add(key:=UCase(anObjectEntry.XID), value:=anObjectEntry)
                    Else
                        Assert(False)

                    End If
                End If
            Next m

            '**********************************************************
            '**** SPECIAL HANDLING OF tblschedules -> Milestones
            '**********************************************************
            If TABLE.ID.ToLower = aSchedule.PrimaryTableID.ToLower Then
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
                    aSchedule = Schedule.Retrieve(UID:=anUID, updc:=anUPDC)
                    If aSchedule IsNot Nothing Then
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
            For Each objectname In aCompTableDir.Keys

                ' get the Entries
                aTableName = CStr(objectname)
                anEntryDir = aCompTableDir.Item(key:=aTableName)
                anObjectEntry = anEntryDir.First.Value      'first item
                compIDFieldname = DirectCast(anObjectEntry, ObjectCompoundEntry).CompoundIDFieldname
                compValueFieldName = DirectCast(anObjectEntry, ObjectCompoundEntry).CompoundValueFieldname

                ' look up the keys
                compoundKeys = DirectCast(anObjectEntry, ObjectCompoundEntry).CompoundRelation
                If Not IsArrayInitialized(compoundKeys) Then
                    Call CoreMessageHandler(arg1:=anObjectEntry.Entryname, message:="no compound relation found for entryname", subname:="XChangeConfiguration.fillMappingWithCompounds")
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
                If LCase(aTableName) = ScheduleMilestone.constTableID.ToLower And specialHandling Then

                    For Each anObjectEntry In TABLE.GetEntries
                        'aTableEntry = m
                        If anObjectEntry.XID <> "" And anObjectEntry.IsCompound Then
                            aCompValue = aSchedule.GetMilestoneValue(ID:=anObjectEntry.XID)
                            'Set aTableEntry = anEntryDir.Item(Key:=LCase(aCompField)) -> should be the same
                            'aConfigmember = Me.AttributeByFieldName(entryname:=aTableEntry.ID, objectname:=aTableEntry.Objectname)
                            aVAlue = Nothing
                            ' map it back -> set values which are not set (e.g. other keys)
                            If Not aConfigmember Is Nothing Then
                                ' save old value
                                If ORIGMAPPING.ContainsKey(key:=aConfigmember.Ordinal.Value) Then
                                    Call ORIGMAPPING.Remove(key:=aConfigmember.Ordinal.Value)
                                End If
                                'Call aConfigmember.convertValue4DB(aCompValue, aVAlue)    '-> MAPPING SHOULD BE HOST DATA
                                Call ORIGMAPPING.Add(key:=aConfigmember.Ordinal.Value, value:=aVAlue)

                                ' overload depending otRead and not PrimaryKey or otUpdate
                                ' run the original DB Value (runXCHange makes s 4DB too)
                                'Call aConfigmember.runXChange(MAPPING:=MAPPING, VARIABLE:=aCompValue, MSGLOG:=MSGLOG)

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
                        Call CoreMessageHandler(subname:="XChangeConfiguration.fillMappingWithCompounds", arg1:=ConstDefaultCompoundIndexName,
                                              message:=" the compound index is not found ",
                                               messagetype:=otCoreMessageType.InternalError, objectname:=aTableName)
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
                                If ORIGMAPPING.ContainsKey(key:=aConfigmember.Ordinal.Value) Then
                                    Call ORIGMAPPING.Remove(key:=aConfigmember.Ordinal.Value)
                                End If
                                'Call aConfigmember.convertValue4DB(aCompValue, aVAlue)    '-> MAPPING SHOULD BE HOST DATA

                                Call ORIGMAPPING.Add(key:=aConfigmember.Ordinal.Value, value:=aVAlue)

                                ' overload depending otXChangeCommandType.Read and not PrimaryKey or otUpdate
                                ' run the original DB Value (runXCHange makes s 4DB too)
                                'Call aConfigmember.runXChange(MAPPING:=MAPPING, VARIABLE:=aCompValue, MSGLOG:=MSGLOG)
                            End If
                        End If
                    Next aRecord
                End If

            Next objectname

            fillMappingWithCompounds = True
        End Function


    End Class

End Namespace