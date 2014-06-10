REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** queried object enumeration for ORM iormPersistables 
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2014-03-14
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2014
REM ***********************************************************************************************************************************************
Option Explicit On
Imports System.Collections.Generic
Imports System.IO
Imports System.Diagnostics.Debug
Imports OnTrack.Commons


Namespace OnTrack.Database

    ''' <summary>
    ''' Query based parts of the ormDataObject
    ''' </summary>
    ''' <remarks></remarks>
    Partial Public MustInherit Class ormDataObject
        ''' <summary>
        ''' Returns a Query Enumeration
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetQuery(name As String) As iormQueriedEnumeration Implements iormQueriable.GetQuery
            Return Me.ObjectDefinition.GetQuery(name:=name)
        End Function

        ''' <summary>
        ''' Static Function ALL returns a Collection of all objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function AllDataObject(Of T As {iormInfusable, iormPersistable, New})(Optional ID As String = "All", _
                                                                                  Optional domainid As String = Nothing,
                                                                                   Optional where As String = "", _
                                                                                   Optional orderby As String = "", _
                                                                                   Optional parameters As List(Of ormSqlCommandParameter) = Nothing, _
                                                                                   Optional deleted As Boolean = False) _
                                                                               As List(Of T)
            Dim theObjectList As New List(Of T)
            Dim aRecordCollection As New List(Of ormRecord)
            Dim aStore As iormDataStore
            Dim anObject As New T
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID

            '** is a session running ?!
            If Not CurrentSession.IsRunning AndAlso Not CurrentSession.IsStartingUp Then
                Call CoreMessageHandler(message:="data object cannot be retrieved - start session to database first", _
                                        objectname:=anObject.ObjectID, _
                                        subname:="ormDataObject.All", messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If

            '** check on the operation right for this object for the current username (might be that during session startup otdb username is not set)
            If Not CurrentSession.IsStartingUp AndAlso Not ot.GetBootStrapObjectClassIDs.Contains(anObject.ObjectID) _
                AndAlso Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadOnly, domainid:=domainID, _
                                                                objecttransactions:={anObject.ObjectID & "." & ConstOPInject}) Then
                '** request authorizartion
                If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadOnly, domainID:=domainID, _
                                                                            username:=CurrentSession.Username, _
                                                                            objecttransactions:={anObject.ObjectID & "." & ConstOPInject}) Then
                    Call CoreMessageHandler(message:="data object cannot be retrieved - permission denied to user", _
                                            objectname:=anObject.ObjectID, arg1:=ConstOPInject, username:=CurrentSession.Username, _
                                            subname:="ormDataObject.Retrieve", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End If
            End If

            Try
                aStore = anObject.PrimaryTableStore
                If parameters Is Nothing Then
                    parameters = New List(Of ormSqlCommandParameter)
                End If
                ''' build domain behavior and deleteflag
                ''' 
                If anObject.ObjectHasDomainBehavior Then
                    If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID
                    ''' add where
                    If Not String.IsNullOrWhiteSpace(where) Then where &= " AND "
                    where &= String.Format(" ([{0}] = @{0} OR [{0}] = @Global{0})", ConstFNDomainID)
                    ''' add parameters
                    If parameters.Find(Function(x)
                                           Return x.ID.ToUpper = "@" & ConstFNDomainID.ToUpper
                                       End Function) Is Nothing Then
                        parameters.Add(New ormSqlCommandParameter(id:="@" & ConstFNDomainID, columnname:=ConstFNDomainID, _
                                                                  tablename:=anObject.primaryTableID, value:=domainID)
                                       )
                    End If
                    If parameters.Find(Function(x)
                                           Return x.ID.ToUpper = "@Global" & ConstFNDomainID.ToUpper
                                       End Function
                                      ) Is Nothing Then
                        parameters.Add(New ormSqlCommandParameter(id:="@Global" & ConstFNDomainID, columnname:=ConstFNDomainID, _
                                                                  tablename:=anObject.primaryTableID, value:=ConstGlobalDomain)
                                       )
                    End If
                End If
                ''' delete 
                ''' 
                If anObject.ObjectHasDeletePerFlagBehavior Then
                    If Not String.IsNullOrWhiteSpace(where) Then where &= " AND "
                    where &= String.Format(" [{0}] = @{0}", ConstFNIsDeleted)
                    If parameters.Find(Function(x)
                                           Return x.ID.ToUpper = "@" & ConstFNIsDeleted.ToUpper
                                       End Function
                                       ) Is Nothing Then

                        parameters.Add(New ormSqlCommandParameter(id:="@" & ConstFNIsDeleted, columnname:=ConstFNIsDeleted, tablename:=anObject.primaryTableID, value:=deleted)
                                       )
                    End If
                End If

                ''' get the records
                aRecordCollection = aStore.GetRecordsBySqlCommand(id:=ID, wherestr:=where, orderby:=orderby, parameters:=parameters)
                If aRecordCollection Is Nothing Then
                    CoreMessageHandler(message:="no records returned due to previous errors", subname:="ormDataObject.AllDataObject", arg1:=ID, _
                                        objectname:=anObject.ObjectID, tablename:=anObject.primaryTableID, messagetype:=otCoreMessageType.InternalError)
                    Return theObjectList
                End If
                Dim aDomainRecordCollection As New Dictionary(Of String, ormRecord)
                Dim pknames = aStore.TableSchema.PrimaryKeys
                Dim domainBehavior As Boolean = False

                If anObject.ObjectHasDomainBehavior And domainID <> ConstGlobalDomain Then
                    domainBehavior = True
                End If
                '*** phase I: get all records and store either the currentdomain or the globaldomain if on domain behavior
                '***
                For Each aRecord As ormRecord In aRecordCollection

                    ''' domain behavior and not on global domain
                    ''' 
                    If domainBehavior Then
                        '** build pk key
                        Dim pk As String = ""
                        For Each acolumnname In pknames
                            If acolumnname <> ConstFNDomainID Then pk &= aRecord.GetValue(index:=acolumnname).ToString & ConstDelimiter
                        Next
                        If aDomainRecordCollection.ContainsKey(pk) Then
                            Dim anotherRecord = aDomainRecordCollection.Item(pk)
                            If anotherRecord.GetValue(ConstFNDomainID).ToString = ConstGlobalDomain Then
                                aDomainRecordCollection.Remove(pk)
                                aDomainRecordCollection.Add(key:=pk, value:=aRecord)
                            End If
                        Else
                            aDomainRecordCollection.Add(key:=pk, value:=aRecord)
                        End If
                    Else
                        ''' just build the list
                        Dim atargetobject As New T
                        If InfuseDataObject(record:=aRecord, dataobject:=atargetobject, mode:=otInfuseMode.OnInject Or otInfuseMode.OnDefault) Then
                            theObjectList.Add(atargetobject)
                        End If
                    End If
                Next

                '** phase II: if on domainbehavior then get the objects out of the active domain entries
                '**
                If domainBehavior Then
                    For Each aRecord In aDomainRecordCollection.Values
                        Dim atargetobject As New T
                        If ormDataObject.InfuseDataObject(record:=aRecord, dataobject:=TryCast(atargetobject, iormInfusable), _
                                                          mode:=otInfuseMode.OnInject Or otInfuseMode.OnDefault) Then
                            theObjectList.Add(DirectCast(atargetobject, iormPersistable))
                        End If
                    Next
                End If

                ''' return the ObjectsList
                Return theObjectList

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="ormDataObject.All(of T)")
                Return theObjectList
            End Try


        End Function
    End Class
    ''' <summary>
    ''' Enumerator for QueryEnumeration
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ormQueryEnumerator
        Implements IEnumerator
        Implements IDisposable

        Private _queriedEnumeration As iormQueriedEnumeration
        Private _counter As Long = -1

        Public Sub New(qry As iormQueriedEnumeration)
            _queriedEnumeration = qry
        End Sub
        Public ReadOnly Property Current As Object Implements IEnumerator.Current
            Get
                If _counter >= 0 And _counter < _queriedEnumeration.Count Then Return _queriedEnumeration.GetObject(_counter)
                ' throw else
                Throw New InvalidOperationException()
            End Get
        End Property

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            _counter += 1
            Return (_counter < _queriedEnumeration.Count)
            ' throw else
            Throw New InvalidOperationException()
        End Function

        Public Sub Reset() Implements IEnumerator.Reset
            _queriedEnumeration.Reset()
            _counter = -1
        End Sub


#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
    ''' <summary>
    ''' a queried enumeration object runs a query and build a enumeration of iormpersistable objects
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ormQueriedEnumeration
        Implements iormQueriedEnumeration
        Implements IEnumerable

        Private _id As String
        Private _objecttype As System.Type
        Private _objectid As String
        Private _otherobjectids As New List(Of String)
        Private _objectentrienamess As New List(Of String)
        Private _objectentriesOrdinal As New Dictionary(Of UShort, String) ' dictionary of Ordinal to ObjectEntryname
        Private _select As ormSqlSelectCommand
        Private _parametervalues As New Dictionary(Of String, Object)

        Private _runTimestamp As DateTime
        Private _run As Boolean = False
        Private _records As New List(Of ormRecord)

        ''' <summary>
        '''  Parameters
        ''' </summary>
        ''' <remarks></remarks>
        Private _steps As UShort = 0
        Private _domainid As String = ""
        Private _deleted As Boolean?
        Private _isObjectEnumerated = True

        Private _qrystopwatch As New Stopwatch
        Private _qryStart As DateTime
        Private _qryEnd As DateTime
        Private _qrycount As ULong

        ''' <summary>
        ''' constructors
        ''' </summary>
        ''' <remarks>
        ''' set domainid if bound to a domain otherwise currentdomain
        ''' </remarks>
        Public Sub New(type As System.Type, _
                       Optional id As String = "", _
                       Optional domainID As String = Nothing,
                       Optional where As String = "", _
                       Optional orderby As String = "", _
                       Optional tablenames As String() = Nothing, _
                       Optional parameters As List(Of ormSqlCommandParameter) = Nothing, _
                       Optional deleted As Boolean? = Nothing)

            ''' check the id
            ''' 
            If id <> "" Then
                _id = id
            Else
                _id = Guid.NewGuid.ToString
            End If

            ''' create a sql select command
            ''' 
            _select = New ormSqlSelectCommand(id)
            If String.IsNullOrWhiteSpace(domainID) Then domainID = ConstGlobalDomain
            Me.Domainid = domainID
            Me.Where = where
            Me.Orderby = orderby
            If parameters IsNot Nothing Then Me.Parameters = parameters
            If deleted.HasValue Then Me.Deleted = deleted

            ''' set the resulted object type
            ''' 
            _isObjectEnumerated = SetObjectType(type)

            ''' Check Tablenames
            If tablenames IsNot Nothing AndAlso CheckTablenames(tablenames) Then
                Throw New ormException("instance creation error for " & _objecttype.Name & " for tables " & tablenames.ToArray.ToString)
            End If
        End Sub

        Public Sub New(type As System.Type, command As ormSqlSelectCommand, Optional id As String = "")
            ''' check the id
            ''' 
            If id <> "" Then
                _id = id
            Else
                _id = Guid.NewGuid.ToString
            End If

            ''' set the resulted object type
            ''' 
            _isObjectEnumerated = SetObjectType(type)

            ''' Check tablename
            ''' 
            If CheckTablenames(command.TableIDs) Then
                Throw New ormException("instance creation error for " & _objecttype.Name & " for tables " & command.TableIDs.ToArray.ToString)
            End If
            _select = command
        End Sub

#Region "Properties"
        ''' <summary>
        ''' sets entry order for 
        ''' </summary>
        ''' <param name="ordered"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ObjectEntriesNames As IEnumerable(Of String) Implements iormQueriedEnumeration.ObjectEntryNames
            Get
                Return _objectentriesOrdinal.Values
            End Get
            Set(value As IEnumerable(Of String))
                _objectentriesOrdinal.Clear()
                _objectentrienamess.Clear()
                Dim i = 1
                For Each aName In value
                    If Not _objectentrienamess.Contains(aName) Then
                        _objectentriesOrdinal.Add(i, aName)
                        _objectentrienamess.Add(aName)
                        i += 1
                    Else
                        CoreMessageHandler(message:="entry name is not in query (" & _id & ") results entry names", arg1:=aName, subname:="ormQueriedEnumeration.EntryOrder", messagetype:=otCoreMessageType.InternalError)
                    End If
                Next
            End Set
        End Property

        ''' <summary>
        ''' returns the elapsed timespan in milliseconds for the query to fetch all records
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property QryElapsedMilliseconds As Long
            Get
                If _run Then Return _qrystopwatch.ElapsedMilliseconds
                Return 0
            End Get
        End Property
        ''' <summary>
        ''' Gets the qrycount.
        ''' </summary>
        ''' <value>The qrycount.</value>
        Public ReadOnly Property Qrycount() As ULong
            Get
                Return Me._qrycount
            End Get
        End Property

        ''' <summary>
        ''' Gets the qry end.
        ''' </summary>
        ''' <value>The qry end.</value>
        Public ReadOnly Property QryEnd() As DateTime
            Get
                Return Me._qryEnd
            End Get
        End Property

        ''' <summary>
        ''' Gets the qry start.
        ''' </summary>
        ''' <value>The qry start.</value>
        Public ReadOnly Property QryStart() As DateTime
            Get
                Return Me._qryStart
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the is object enumerated.
        ''' </summary>
        ''' <value>The is object enumerated.</value>
        Public Property AreObjectsEnumerated() As Object Implements iormQueriedEnumeration.AreObjectsEnumerated
            Get
                Return Me._isObjectEnumerated
            End Get
            Private Set(value As Object)
                Me._isObjectEnumerated = value
            End Set
        End Property

        ''' <summary>
        ''' returns the size of the records list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Count As ULong Implements iormQueriedEnumeration.Count
            Get
                If Not _run Then
                    If Not Me.Run() Then
                        CoreMessageHandler(message:="failed to run query", subname:="ormQueriedEnumeration.GetObject", messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    End If
                End If
                Return _records.Count
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the steps.
        ''' </summary>
        ''' <value>The steps.</value>
        Public Property Steps() As UShort
            Get
                Return Me._steps
            End Get
            Set(value As UShort)
                Me._steps = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the deleted.
        ''' </summary>
        ''' <value>The deleted.</value>
        Public Property Deleted() As Boolean
            Get
                Return Me._deleted
            End Get
            Set(value As Boolean)
                Me._deleted = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the parameters.
        ''' </summary>
        ''' <value>The parameters.</value>
        Public Property Parameters() As List(Of ormSqlCommandParameter)
            Get
                Return Me._select.Parameters
            End Get
            Set(value As List(Of ormSqlCommandParameter))
                For Each aP In value
                    If _select.Parameters.Find(Function(x)
                                                   Return x.ID = aP.ID
                                               End Function) Is Nothing Then
                        _select.AddParameter(aP)
                    End If
                Next
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the parameters.
        ''' </summary>
        ''' <value>The parameters.</value>
        Public Property Tablenames() As List(Of String)
            Get
                Return Me._select.TableIDs
            End Get
            Set(value As List(Of String))
                If value IsNot Nothing AndAlso CheckTablenames(value) Then
                    Throw New ormException("instance creation error for " & _objecttype.Name & " for tables " & value.ToArray.ToString)
                End If
                For Each aTablename In value
                    If _select.TableIDs.Contains(aTablename.ToUpper) Then
                        _select.AddTable(aTablename, addAllFields:=True)
                    End If
                Next
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the orderby.
        ''' </summary>
        ''' <value>The orderby.</value>
        Public Property Orderby() As String
            Get
                Return _select.OrderBy
            End Get
            Set(value As String)
                _select.OrderBy = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the where.
        ''' </summary>
        ''' <value>The where.</value>
        Public Property Where() As String
            Get
                Return _select.Where
            End Get
            Set(value As String)
                _select.Where = Where
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the static domainid in this query.
        ''' </summary>
        ''' <value>The domainid.</value>
        Public Property Domainid() As String
            Get
                Return Me._domainid
            End Get
            Set(value As String)
                Me._domainid = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the id.
        ''' </summary>
        ''' <value>The id.</value>
        Public ReadOnly Property Id() As String Implements iormQueriedEnumeration.ID
            Get
                Return Me._id
            End Get
        End Property
        ''' <summary>
        ''' true if the query was run
        ''' </summary>
        ''' <value></value>
        Public ReadOnly Property HasRun() As Boolean
            Get
                Return Me._run
            End Get
        End Property
        ''' <summary>
        ''' Gets the run timestamp.
        ''' </summary>
        ''' <value>The run timestamp.</value>
        Public ReadOnly Property RunTimestamp() As DateTime
            Get
                Return Me._runTimestamp
            End Get
        End Property

#End Region


        ''' <summary>
        ''' check the tablenames
        ''' </summary>
        ''' <param name="tablenames"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function CheckTablenames(tablenames As IEnumerable(Of String)) As Boolean
            Dim found As Boolean = False

            If _objecttype Is Nothing Then Return True

            ''' check the tablename
            ''' 
            If tablenames IsNot Nothing Then
                ''' check each tablename
                For Each tablename In tablenames
                    Dim theDescriptions = ot.GetObjectClassDescriptionByTable(tablename)
                    If theDescriptions Is Nothing Then
                        CoreMessageHandler(message:="The supplied QueriedEnumeration type '" & _objecttype.Name & "' has no class description for table '" & tablename & "'", subname:="ormQueriedEnumeration.CheckTablename", _
                                          messagetype:=otCoreMessageType.InternalError)
                        Return False
                    Else
                        For Each aDescription In theDescriptions
                            If Not _otherobjectids.Contains(aDescription.ObjectAttribute.ID) Then _otherobjectids.Add(aDescription.ObjectAttribute.ID)
                        Next
                    End If
                Next
                ''' conclude
                ''' 
                If Not _otherobjectids.Contains(_objectid.ToUpper) Then
                    CoreMessageHandler(message:="The supplied QueriedEnumeration type '" & _objecttype.Name & "' does not use the table '" & tablenames.ToString & "'", subname:="ormQueriedEnumeration.CheckTablename", _
                                        messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
            End If
        End Function

        ''' <summary>
        ''' set the Object Type
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function SetObjectType(type As System.Type) As Boolean
            ''' Check Type
            ''' 
            If type.GetInterface(name:=GetType(iormPersistable).Name) IsNot Nothing Then
                Dim aDescription = ot.GetObjectClassDescription(type)
                If aDescription Is Nothing Then
                    Throw New ormException(message:="The supplied type '" & type.Name & "' has not been found in the Class Repository ")
                Else
                    _objectid = aDescription.ObjectAttribute.ID
                    _objecttype = type
                    Dim aList As New List(Of String)
                    For Each anEntry In Me.GetObjectEntries
                        If anEntry.IsMapped Then aList.Add(anEntry.Entryname)
                    Next
                    Me.ObjectEntriesNames = aList
                    Return True
                End If
            Else
                Throw New ormException(message:="The supplied type '" & type.Name & "' is not implementing " & GetType(iormPersistable).Name)
            End If
        End Function
        ''' <summary>
        ''' returns the primary ClassDescription
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassDescription() As ObjectClassDescription Implements iormQueriedEnumeration.GetObjectClassDescription
            Return ot.GetObjectClassDescriptionByID(_objectid)
        End Function
        ''' <summary>
        ''' returns the primary Object Definition
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectDefinition() As ObjectDefinition Implements iormQueriedEnumeration.GetObjectDefinition
            Return CurrentSession.Objects.GetObject(_objectid)
        End Function
        ''' <summary>
        ''' returns a list of iobject entries returned by this Queried Enumeration
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectEntries() As IOrderedEnumerable(Of iormObjectEntry) Implements iormQueriedEnumeration.GetObjectEntries
            Dim anObjectDefinition As ObjectDefinition = CurrentSession.Objects.GetObject(objectid:=_objectid)
            Return anObjectDefinition.GetOrderedEntries
        End Function
        ''' <summary>
        ''' returns a list of iobject entries returned by this Queried Enumeration
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectEntry(name As String) As iormObjectEntry Implements iormQueriedEnumeration.GetObjectEntry
            Dim anObjectDefinition As ObjectDefinition = CurrentSession.Objects.GetObject(objectid:=_objectid)
            Return anObjectDefinition.GetEntry(entryname:=name)
        End Function

        ''' <summary>
        ''' sets the value of parameter
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetValue(name As String, value As Object) As Boolean Implements iormQueriedEnumeration.SetValue
            If _parametervalues.ContainsKey(name) Then
                Return False
            Else
                _parametervalues.Add(name, value)
            End If
        End Function
        ''' <summary>
        ''' sets the value of parameter
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetValue(name As String, ByRef value As Object) As Boolean Implements iormQueriedEnumeration.GetValue
            If _parametervalues.ContainsKey(name) Then
                value = _parametervalues.Item(key:=name)
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' resets the result but not the query itself
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Reset() As Boolean Implements iormQueriedEnumeration.Reset
            If Not _run Then
                _run = False
                _records.Clear()
                _runTimestamp = Nothing
                _parametervalues.Clear()
                Return True
            End If
            Return False
        End Function
        ''' <summary>
        ''' returns an infused object out of the zero-based number or results
        ''' </summary>
        ''' <param name="no"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObject(no As ULong) As iormPersistable Implements iormQueriedEnumeration.GetObject
            If Not _run Then
                If Not Me.Run() Then
                    CoreMessageHandler(message:="failed to run query", subname:="ormQueriedEnumeration.GetObject", messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If
            End If
            If _run Then
                If no < _records.Count Then
                    Dim newObject As iormPersistable = ot.CreateDataObjectInstance(_objecttype)
                    If ormDataObject.InfuseDataObject(_records.ElementAt(no), dataobject:=newObject, mode:=otInfuseMode.OnInject) Then
                        Return newObject
                    End If
                End If
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' adds object out of the zero-based number or results
        ''' </summary>
        ''' <param name="no"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddObject(dataobject As iormPersistable, Optional ByRef no As ULong? = Nothing) As Boolean Implements iormQueriedEnumeration.AddObject
            If Not _run Then
                If Not Me.Run() Then
                    CoreMessageHandler(message:="failed to run query", subname:="ormQueriedEnumeration.GetObject", messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
            End If
            If _run Then
                _records.Add(dataobject.Record)
                If no.HasValue Then no = _records.Count - 1
            End If
            Return True
        End Function
        ''' <summary>
        ''' adds object out of the zero-based number or results
        ''' </summary>
        ''' <param name="no"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function DeleteObject(no As ULong) As Boolean Implements iormQueriedEnumeration.DeleteObject
            If Not _run Then
                If Not Me.Run() Then
                    CoreMessageHandler(message:="failed to run query", subname:="ormQueriedEnumeration.DeleteObject", messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If
            End If
            If _run Then
                Dim newObject As iormPersistable = ot.CreateDataObjectInstance(_objecttype)
                ''' this will get the object from the cache
                If ormDataObject.InfuseDataObject(_records.ElementAt(no), dataobject:=newObject, mode:=otInfuseMode.OnInject) Then
                    Return newObject.Delete
                End If
                ' keep the record - all references would be lost
                ' _records.RemoveAt(no)
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' returns the zero-based number of records
        ''' </summary>
        ''' <param name="no"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetRecord(no As ULong) As ormRecord Implements iormQueriedEnumeration.getRecord
            If Not _run Then
                If Not Me.Run() Then
                    CoreMessageHandler(message:="failed to run query", subname:="ormQueriedEnumeration.GetRecord", messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If
            End If
            If _run Then
                If no < _records.Count Then Return _records.ElementAt(no)
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' run the query
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Run(Optional domainid As String = Nothing) As Boolean

            ''' prepare
            ''' 
            If Not _select.Prepared Then
                If Not _select.Prepare Then
                    CoreMessageHandler(message:="sql select command couldnot be prepared", subname:="ormQueriedEnumeration.Run", messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
            End If

            If _select.Prepared Then
                ''' instance just for some settings
                ''' should be reworked
                Dim anObjectDefinition As ObjectDefinition = CurrentSession.Objects.GetObject(_objectid)
                Dim hasDomainBehavior As Boolean = False
                If anObjectDefinition IsNot Nothing Then
                    hasDomainBehavior = anObjectDefinition.HasDomainBehavior
                End If
                If hasDomainBehavior Then
                    If String.IsNullOrWhiteSpace(domainid) Then domainid = Me.Domainid
                    If String.IsNullOrWhiteSpace(domainid) Then domainid = CurrentSession.CurrentDomainID
                    ''' set domain parameter
                    Dim aDomainIDParameter = _select.Parameters.Find(Function(x)
                                                                         Return x.ID.ToUpper = "@" & Domain.ConstFNDomainID.ToUpper
                                                                     End Function)
                    If aDomainIDParameter IsNot Nothing Then
                        If _parametervalues.ContainsKey(key:="@" & Domain.ConstFNDomainID.ToUpper) Then
                            _parametervalues.Remove(key:="@" & Domain.ConstFNDomainID.ToUpper)
                        End If
                        _parametervalues.Add(key:="@" & Domain.ConstFNDomainID.ToUpper, value:=domainid)
                        aDomainIDParameter.Value = domainid
                    End If
                End If

                ''' run the statement
                ''' 
                _qryStart = DateTime.Now
                _qrystopwatch.Start()
                Dim aRecordCollection = _select.RunSelect(parametervalues:=_parametervalues)
                If aRecordCollection Is Nothing Then
                    CoreMessageHandler(message:="no records returned due to previous errors", subname:="ormQueriedEnumeration.Run", arg1:=Me.Id, _
                                        objectname:=_objectid, tablename:=_select.TableIDs.ToString, messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
                _qryEnd = DateTime.Now
                _qrystopwatch.Stop()
                _qrycount = aRecordCollection.Count
                Call CoreMessageHandler(message:="query " & Me.Id & " run on " & Format(QryStart, "yyyy-mm-dd hh:mm:ss") & " for " & _
                                    _qrystopwatch.ElapsedMilliseconds & " ms and returned " & _qrycount & " records", _
                                   messagetype:=otCoreMessageType.InternalInfo, subname:="ormQueriedEnumeration.Run")

                If hasDomainBehavior And domainid <> ConstGlobalDomain Then

                    Dim aDomainRecordCollection As New Dictionary(Of String, ormRecord)
                    Dim pknames = CurrentSession.CurrentDBDriver.GetTableSchema(tableID:=_select.TableIDs.First).PrimaryKeys
                    '*** get all records and store either the currentdomain or the globaldomain if on domain behavior
                    '***
                    For Each aRecord As ormRecord In aRecordCollection

                        '** build pk key
                        Dim pk As String = ""
                        For Each acolumnname In pknames
                            If acolumnname <> Commons.Domain.ConstFNDomainID Then pk &= aRecord.GetValue(index:=acolumnname).ToString & ConstDelimiter
                        Next

                        If aDomainRecordCollection.ContainsKey(pk) Then
                            Dim anotherRecord = aDomainRecordCollection.Item(pk)
                            If anotherRecord.GetValue(Domain.ConstFNDomainID).ToString = ConstGlobalDomain Then
                                aDomainRecordCollection.Remove(pk)
                                aDomainRecordCollection.Add(key:=pk, value:=aRecord)
                            End If
                        Else
                            aDomainRecordCollection.Add(key:=pk, value:=aRecord)
                        End If
                    Next

                    ''' set the result
                    _records = aDomainRecordCollection.Values.ToList
                Else
                    ''' set the result
                    _records = aRecordCollection
                End If

                _run = True
                _runTimestamp = DateTime.Now
            Else
                _run = False
                _runTimestamp = DateTime.Now
            End If

            Return _run
        End Function
        ''' <summary>
        ''' returns a Enumerator over the QueriedEnumeration
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return New ormQueryEnumerator(Me)
        End Function
    End Class
End Namespace
