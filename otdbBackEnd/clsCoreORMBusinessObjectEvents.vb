
REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** ORM DATA OBJECT CLASSES
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2014-01-31
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************''' <summary>

Option Explicit On

Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Reflection

Namespace OnTrack.Database
   
    Partial Public MustInherit Class ormBusinessObject
        ''' <summary>
        ''' Events
        ''' </summary>
        ''' <remarks></remarks>
        Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

        '** Lifecycle Events
       
        Public Shared Event ClassOnRetrieving(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnOverloaded(sender As Object, e As ormDataObjectOverloadedEventArgs)
        Public Shared Event ClassOnRetrieved(sender As Object, e As ormDataObjectEventArgs)

        Public Event OnInjected(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnInjected
        Public Event OnInjecting(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnInjecting

        Public Shared Event ClassOnPersisting(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnPersisted(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnPersisting(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnPersisting
        Public Event OnPersisted(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnPersisted

        Public Shared Event ClassOnUnDeleting(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnUnDeleted(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnUnDeleting(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnUnDeleting
        Public Event OnUnDeleted(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnUnDeleted

        Public Shared Event ClassOnDeleting(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnDeleted(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnDeleting(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnDeleting
        Public Event OnDeleted(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnDeleted

        Public Shared Event ClassOnCreating(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnCreated(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnCreating(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnCreating
        Public Event OnCreated(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnCreated
        Public Event OnCreateDefaultValuesNeeded(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnCreateDefaultValuesNeeded

        Public Event OnCloning(sender As Object, e As ormDataObjectCloneEventArgs) Implements iormCloneable.OnCloning
        Public Event OnCloned(sender As Object, e As ormDataObjectCloneEventArgs) Implements iormCloneable.OnCloned
        Public Shared Event ClassOnCloning(sender As Object, e As ormDataObjectCloneEventArgs)
        Public Shared Event ClassOnCloned(sender As Object, e As ormDataObjectCloneEventArgs)

        '* uniqueness Class Event
        Public Shared Event ClassOnCheckingUniqueness(sender As Object, e As ormDataObjectEventArgs)

        '* Validation Events
        Public Event OnEntryValidating(sender As Object, e As ormDataObjectEntryEventArgs) Implements iormValidatable.OnEntryValidating
        Public Event OnEntryValidated(sender As Object, e As ormDataObjectEntryEventArgs) Implements iormValidatable.OnEntryValidated
        Public Event OnValidating(sender As Object, e As ormDataObjectEventArgs) Implements iormValidatable.OnValidating
        Public Event OnValidated(sender As Object, e As ormDataObjectEventArgs) Implements iormValidatable.OnValidated


        '** Events for the Switch from Runtime Mode on to Off
       
        '** ObjectMessage Added to Log
        Public Event OnObjectMessageAdded(sender As Object, e As ObjectMessageLog.EventArgs)

        ''' <summary>
        ''' Handler cascaded the OnObjectMessageAdded Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ormDataObject_OnObjectMessageAdded(sender As Object, e As ObjectMessageLog.EventArgs) Handles _objectmessagelog.OnObjectMessageAdded
            RaiseEvent OnObjectMessageAdded(sender:=sender, e:=e)
        End Sub
        
        ''' <summary>
        ''' raises the PropetfyChanged Event
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Sub RaiseObjectEntryChanged(entryname As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(entryname))
        End Sub

        ''' <summary>
        ''' Event Handler for defaultValues
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub ormDataObject_OnDefaultValuesNeeded(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnCreateDefaultValuesNeeded
            Dim result As Boolean = True

            '** set the default values of the object
            If Not CurrentSession.IsBootstrappingInstallationRequested AndAlso Not CurrentSession.IsStartingUp Then
                For Each anEntry In e.DataObject.ObjectDefinition.GetEntries
                    ' only the columns
                    If anEntry.IsColumn Then
                        Dim anColumnEntry As ObjectContainerEntry = TryCast(anEntry, ObjectContainerEntry)
                        If anColumnEntry IsNot Nothing And Not e.Record.HasIndex(anColumnEntry.ContainerID & "." & anColumnEntry.ContainerEntryName) Then
                            '' if a default value is neded is decided in the defaultvalue property
                            '' it might be nothing if nullable is true
                            result = result And e.Record.SetValue(anColumnEntry.ContainerID & "." & anColumnEntry.ContainerEntryName, value:=anColumnEntry.Defaultvalue)
                        End If
                    End If
                Next
            Else
                ''' during bootstrapping install or starting up just take the class description values
                ''' 
                For Each anEntry In Me.ObjectClassDescription.ObjectEntryAttributes
                    ' only the columns
                    If anEntry.EntryType = otObjectEntryType.ContainerEntry And Not e.Record.HasIndex(anEntry.ContainerID & "." & anEntry.ContainerEntryName) Then
                        If anEntry.HasValueDefaultValue Then
                            result = result And e.Record.SetValue(anEntry.ContainerID & "." & anEntry.ContainerEntryName, value:=Converter.Object2otObject(anEntry.DefaultValue, anEntry.DataType))
                        ElseIf Not anEntry.HasValueIsNullable OrElse (anEntry.HasValueIsNullable AndAlso Not anEntry.IsNullable) Then
                            result = result And e.Record.SetValue(anEntry.ContainerID & "." & anEntry.ContainerEntryName, value:=ot.GetDefaultValue(anEntry.DataType))
                        End If
                    End If
                Next
            End If


            e.Result = result
            e.Proceed = True
        End Sub

        ''' <summary>
        ''' Event Handler for initializing
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ormBusinessObject_OnInitializing(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnInitializing
            ''' set the status of the tables if not infused already
            ''' 
            If _tableisloaded Is Nothing OrElse _tableisloaded.Length = 0 Then
                ReDim Preserve _tableisloaded(Me.TableIDs.GetUpperBound(0))
                '** set all tables to be unloaded
                ' Array.ForEach(Of Boolean)(_tableisloaded, Function(x) x = False) -> do not overwrite true
            End If
            ''' get new  record if necessary
            ''' STILL we rely on One Table for the Record
            If _record Is Nothing Then
                _record = New ormRecord(Me.TableIDs, dbdriver:=_defaultdbdriver, runtimeOnly:=RunTimeOnly)
                'now we are not runtime only anymore -> set also the table and let's have a fixed structure
            ElseIf Not Me.RunTimeOnly Then
                _record.SetTables(Me.TableIDs, dbdriver:=_defaultdbdriver)
            End If
        End Sub

        ''' <summary>
        ''' Handler for ObjectEntryValidation
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ormBusinessObject_EntryValidationNeeded(sender As Object, e As ormDataObjectEntryValidationEventArgs) Handles Me.EntryValidationNeeded
            If e.Msglog Is Nothing Then e.Msglog = Me.ObjectMessageLog
            e.Result = Validate(entryname:=e.ObjectEntryName, value:=e.Value, msglog:=e.Msglog)
        End Sub
        ''' <summary>
        ''' Handles the OnEntryChanged Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ormBusinessObject_OnEntryChanged(sender As Object, e As ormDataObjectEntryEventArgs) Handles Me.OnEntryChanged
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(e.ObjectEntryName))
        End Sub

        ''' <summary>
        ''' handler for the OnInfused Event 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ormBusinessObject_OnInfused(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnInfused
            '** set all tables to be loaded
            Setloaded()
        End Sub
    End Class

    ''' <summary>
    ''' Event Class for the substitute event
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ormDataObjectOverloadedEventArgs
        Inherits ormDataObjectEventArgs

        Private _globalPKArray As ormDatabaseKey

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(globalPrimaryKey As ormDatabaseKey, domainPrimaryKey As ormDatabaseKey, dataobject As ormBusinessObject, _
                       Optional usecache As Boolean = True, _
                         Optional ByRef msglog As ObjectMessageLog = Nothing,
                        Optional timestamp? As DateTime = Nothing)

            MyBase.New(object:=dataobject, key:=domainPrimaryKey, msglog:=msglog, timestamp:=timestamp, usecache:=usecache)
            _globalPKArray = globalPrimaryKey
        End Sub
        ''' <summary>
        ''' Gets or sets the old object.
        ''' </summary>
        ''' <value>The old object.</value>
        Public ReadOnly Property GlobalPrimaryKey As ormDatabaseKey
            Get
                Return _globalPKArray
            End Get

        End Property
        ''' <summary>
        ''' Gets or sets the old object.
        ''' </summary>
        ''' <value>The old object.</value>
        Public Property DomainPrimaryKey As ormDatabaseKey
            Get
                Return Me.Key
            End Get
            Set(value As ormDatabaseKey)
                Me.Key = value
            End Set

        End Property
    End Class
    ''' <summary>
    ''' Event Class for the clone event
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ormDataObjectCloneEventArgs
        Inherits ormDataObjectEventArgs

        Private _oldObject As ormBusinessObject


        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New([newObject] As ormBusinessObject, _
                       [oldObject] As ormBusinessObject, _
                         Optional ByRef msglog As ObjectMessageLog = Nothing,
                        Optional timestamp? As DateTime = Nothing)

            MyBase.New(object:=newObject, msglog:=msglog, timestamp:=timestamp)
            _oldObject = [oldObject]
        End Sub
        ''' <summary>
        ''' Gets or sets the old object.
        ''' </summary>
        ''' <value>The old object.</value>
        Public ReadOnly Property OldObject() As ormBusinessObject
            Get
                Return Me._oldObject
            End Get
            
        End Property
        ''' <summary>
        ''' Gets or sets the old object.
        ''' </summary>
        ''' <value>The old object.</value>
        Public Property NewObject() As ormBusinessObject
            Get
                Return Me.DataObject
            End Get
            Set(value As ormBusinessObject)
                Me.DataObject = value
            End Set

        End Property
    End Class
    ''' <summary>
    ''' Event Arguments for Data Object Events
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ormDataObjectEventArgs
        Inherits EventArgs

        Protected _Object As ormBusinessObject
        Protected _Record As ormRecord
        Protected _DescribedByAttributes As Boolean = False
        Protected _UseCache As Boolean = False
        Protected _key As ormDatabaseKey
        Protected _relationIDs As List(Of String)
        Protected _Abort As Boolean = False
        Protected _result As Boolean = True
        Protected _domainID As String = ConstGlobalDomain
        Protected _hasDomainBehavior As Boolean = False
        Protected _infusemode As otInfuseMode?
        Protected _timestamp As DateTime? = DateTime.Now
        Protected _runtimeonly As Boolean = False
        Protected _msglog As ObjectMessageLog

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New([object] As ormBusinessObject, _
                       Optional record As ormRecord = Nothing, _
                       Optional describedByAttributes As Boolean = False, _
                        Optional relationID As List(Of String) = Nothing, _
                        Optional domainid As String = Nothing,
                        Optional domainBehavior As Nullable(Of Boolean) = Nothing, _
                          Optional usecache As Nullable(Of Boolean) = Nothing, _
                        Optional key As ormDatabaseKey = Nothing, _
                        Optional runtimeOnly As Boolean = False, _
                        Optional infuseMode As otInfuseMode? = Nothing, _
                         Optional ByRef msglog As ObjectMessageLog = Nothing,
                        Optional timestamp? As DateTime = Nothing)
            _Object = [object]
            _Record = record
            _relationIDs = relationID
            _DescribedByAttributes = describedByAttributes
            If _domainID <> String.empty Then _domainID = domainid
            If domainBehavior.HasValue Then _hasDomainBehavior = domainBehavior
            If usecache.HasValue Then _UseCache = usecache
            If infuseMode.HasValue Then _infusemode = infuseMode
            If timestamp.HasValue Then _timestamp = timestamp
            If key IsNot Nothing Then _key = key
            _result = False
            _runtimeonly = runtimeOnly
            _Abort = False
            If msglog IsNot Nothing Then _msglog = msglog
        End Sub

        ''' <summary>
        ''' Gets or sets the msglog.
        ''' </summary>
        ''' <value>The msglog.</value>
        Public ReadOnly Property Msglog() As ObjectMessageLog
            Get
                Return Me._msglog
            End Get
        End Property

        ''' <summary>
        ''' Gets the timestamp.
        ''' </summary>
        ''' <value>The timestamp.</value>
        Public ReadOnly Property Timestamp() As DateTime?
            Get
                Return Me._timestamp
            End Get
        End Property

        ''' <summary>
        ''' Gets the infusemode.
        ''' </summary>
        ''' <value>The infusemode.</value>
        Public ReadOnly Property Infusemode() As otInfuseMode?
            Get
                Return Me._infusemode
            End Get
        End Property

        ''' <summary>
        ''' Gets the has domain behavior.
        ''' </summary>
        ''' <value>The has domain behavior.</value>
        Public ReadOnly Property HasDomainBehavior() As Boolean
            Get
                Return Me._hasDomainBehavior
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the domain ID.
        ''' </summary>
        ''' <value>The domain ID.</value>
        Public Property DomainID() As String
            Get
                Return Me._domainID
            End Get
            Set(value As String)
                Me._domainID = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the relation ID.
        ''' </summary>
        ''' <value>The relation ID.</value>
        Public Property RelationIDs() As List(Of String)
            Get
                Return Me._relationIDs
            End Get
            Set(value As List(Of String))
                Me._relationIDs = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the result.
        ''' </summary>
        ''' <value>The result.</value>
        Public Property Result() As Boolean
            Get
                Return Me._result
            End Get
            Set(value As Boolean)
                Me._result = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the pkarray.
        ''' </summary>
        ''' <value>The pkarray.</value>
        Public Property Key As ormDatabaseKey
            Get
                Return Me._key
            End Get
            Set(value As ormDatabaseKey)
                Me._key = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the use cache.
        ''' </summary>
        ''' <value>The use cache.</value>
        Public Property UseCache() As Boolean
            Get
                Return Me._UseCache
            End Get
            Set(value As Boolean)
                Me._UseCache = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the abort.
        ''' </summary>
        ''' <value>The abort.</value>
        Public Property AbortOperation() As Boolean
            Get
                Return Me._Abort
            End Get
            Set(value As Boolean)
                Me._Abort = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets if to proceed.
        ''' </summary>
        ''' <value>The abort.</value>
        Public Property Proceed() As Boolean
            Get
                Return Not Me._Abort
            End Get
            Set(value As Boolean)
                Me._Abort = Not value
                Me._result = value
            End Set
        End Property
        ''' <summary>
        ''' Gets the described by attributes.
        ''' </summary>
        ''' <value>The described by attributes.</value>
        Public ReadOnly Property DescribedByAttributes() As Boolean
            Get
                Return Me._DescribedByAttributes
            End Get
        End Property

        ''' <summary>
        ''' Gets the record.
        ''' </summary>
        ''' <value>The record.</value>
        Public ReadOnly Property Record() As ormRecord
            Get
                Return Me._Record
            End Get
        End Property

        ''' <summary>
        ''' Gets the object.
        ''' </summary>
        ''' <value>The object.</value>
        Public Property DataObject() As ormBusinessObject
            Get
                Return Me._Object
            End Get
            Set(value As ormBusinessObject)
                _Object = value
            End Set
        End Property

    End Class

    ''' <summary>
    ''' Event Arguments for the Object Entry Validation Event
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ormDataObjectEntryValidationEventArgs
        Inherits ormDataObjectEntryEventArgs

        Private _validationResult As otValidationResultType = otValidationResultType.Succeeded

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New([object] As ormBusinessObject, _
                        entryname As String,
                        Optional value As Object = Nothing,
                        Optional domainid as string = nothing,
                        Optional ByRef msglog As ObjectMessageLog = Nothing,
                        Optional timestamp? As DateTime = Nothing)
            MyBase.New(object:=[object], entryname:=entryname, value:=value, domainid:=domainID, msglog:=msglog, timestamp:=timestamp)
           
        End Sub
        ''' <summary>
        ''' Gets or sets the validation result.
        ''' </summary>
        ''' <value>The validation result.</value>
        Public Property ValidationResult() As otValidationResultType
            Get
                Return Me._validationResult
            End Get
            Set
                Me._validationResult = Value
            End Set
        End Property

    End Class

    ''' <summary>
    ''' Event Arguments for the Object Validation Event
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ormDataObjectValidationEventArgs
        Inherits ormDataObjectEventArgs

        Private _validationResult As otValidationResultType = otValidationResultType.Succeeded

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New([object] As ormBusinessObject, _
                        Optional domainid as string = nothing,
                        Optional ByRef msglog As ObjectMessageLog = Nothing,
                        Optional timestamp? As DateTime = Nothing)

            MyBase.New(object:=[object], domainid:=domainID, msglog:=msglog, timestamp:=timestamp)

        End Sub
        ''' <summary>
        ''' Gets or sets the validation result.
        ''' </summary>
        ''' <value>The validation result.</value>
        Public Property ValidationResult() As otValidationResultType
            Get
                Return Me._validationResult
            End Get
            Set(value As otValidationResultType)
                Me._validationResult = value
            End Set
        End Property

    End Class

    ''' <summary>
    ''' Event Args for ObjectEntry Events
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ormDataObjectEntryEventArgs
        Inherits EventArgs

        Private _Object As ormBusinessObject
        Private _ObjectEntryName As String
        Private _Abort As Boolean = False
        Private _result As Boolean = True
        Private _domainID As String = ConstGlobalDomain
        Private _timestamp As DateTime? = DateTime.Now
        Private _newvalue As Object
        Private _oldvalue As Object
        Private _msglog As ObjectMessageLog
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New([object] As ormBusinessObject, _
                        entryname As String,
                        Optional value As Object = Nothing,
                        Optional domainid as string = nothing,
                        Optional ByRef msglog As ObjectMessageLog = Nothing,
                        Optional timestamp? As DateTime = Nothing)
            _Object = [object]
            _ObjectEntryName = entryname
            If _domainID <> String.empty Then _domainID = domainID
            'If oldvalue IsNot Nothing Then _oldvalue = oldvalue
            If value IsNot Nothing Then _newvalue = value
            _result = False
            _Abort = False
            If timestamp.HasValue Then _timestamp = timestamp
            If msglog IsNot Nothing Then _msglog = msglog
        End Sub

        ''' <summary>
        ''' Gets or sets the msglog.
        ''' </summary>
        ''' <value>The msglog.</value>
        Public Property Msglog As ObjectMessageLog
            Get
                Return Me._msglog
            End Get
            Set(value As ObjectMessageLog)
                _msglog = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the name of the object entry.
        ''' </summary>
        ''' <value>The name of the object entry.</value>
        Public ReadOnly Property ObjectEntryName() As String
            Get
                Return Me._ObjectEntryName
            End Get

        End Property

        ''' <summary>
        ''' Gets or sets the oldvalue.
        ''' </summary>
        ''' <value>The value.</value>
        'Public ReadOnly Property oldValue() As Object
        '    Get
        '        Return Me._oldvalue
        '    End Get

        'End Property
        ''' <summary>
        ''' Gets or sets the value.
        ''' </summary>
        ''' <value>The value.</value>
        Public Property Value() As Object
            Get
                Return Me._newvalue
            End Get
            Set(value As Object)
                Me._newvalue = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the timestamp.
        ''' </summary>
        ''' <value>The timestamp.</value>
        Public ReadOnly Property Timestamp() As DateTime?
            Get
                Return Me._timestamp
            End Get
        End Property


        ''' <summary>
        ''' Gets or sets the domain ID.
        ''' </summary>
        ''' <value>The domain ID.</value>
        Public Property DomainID() As String
            Get
                Return Me._domainID
            End Get
            Set(value As String)
                Me._domainID = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the result.
        ''' </summary>
        ''' <value>The result.</value>
        Public Property Result() As Boolean
            Get
                Return Me._result
            End Get
            Set(value As Boolean)
                Me._result = value
            End Set
        End Property



        ''' <summary>
        ''' Gets or sets the abort.
        ''' </summary>
        ''' <value>The abort.</value>
        Public Property AbortOperation() As Boolean
            Get
                Return Me._Abort
            End Get
            Set(value As Boolean)
                Me._Abort = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets if to proceed.
        ''' </summary>
        ''' <value>The abort.</value>
        Public Property Proceed() As Boolean
            Get
                Return Not Me._Abort
            End Get
            Set(value As Boolean)
                Me._Abort = Not value
                Me._result = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the object.
        ''' </summary>
        ''' <value>The object.</value>
        Public Property DataObject() As ormBusinessObject
            Get
                Return Me._Object
            End Get
            Set(value As ormBusinessObject)
                _Object = value
            End Set
        End Property

    End Class
    ''' <summary>
    ''' Event Arguments for Data Object Events
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ormDataObjectRelationEventArgs
        Inherits EventArgs


        Private _timestamp As DateTime = DateTime.Now
        Private _relationEventArgs As ormRelationManager.EventArgs

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(ByRef relationMgrEventArgs As ormRelationManager.EventArgs, _
                        Optional timestamp? As DateTime = Nothing)
            _relationEventArgs = relationMgrEventArgs
            If timestamp.HasValue Then _timestamp = timestamp
        End Sub

        ''' <summary>
        ''' Gets or sets the relation attribute.
        ''' </summary>
        ''' <value>The relation attribute.</value>
        Public ReadOnly Property RelationAttribute() As ormRelationAttribute
            Get
                Return _relationEventArgs.RelationAttribute
            End Get
        End Property

        ''' <summary>
        ''' Gets the timestamp.
        ''' </summary>
        ''' <value>The timestamp.</value>
        Public ReadOnly Property Timestamp() As DateTime
            Get
                Return Me._timestamp
            End Get
        End Property

        ''' <summary>
        ''' Gets the infusemode.
        ''' </summary>
        ''' <value>The infusemode.</value>
        Public ReadOnly Property Infusemode() As otInfuseMode?
            Get
                Return _relationEventArgs.InfuseMode
            End Get

        End Property
        ''' <summary>
        ''' Gets or sets the relation ID.
        ''' </summary>
        ''' <value>The relation ID.</value>
        Public ReadOnly Property RelationObjects() As List(Of iormRelationalPersistable)
            Get
                Return _relationEventArgs.Objects
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the relation ID.
        ''' </summary>
        ''' <value>The relation ID.</value>
        Public ReadOnly Property RelationID() As String
            Get
                Return _relationEventArgs.RelationAttribute.Name
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets if to proceed.
        ''' </summary>
        ''' <value>The abort.</value>
        Public Property Finished() As Boolean
            Get
                Return _relationEventArgs.Finished
            End Get
            Set(value As Boolean)
                _relationEventArgs.Finished = value
            End Set
        End Property


        ''' <summary>
        ''' Gets the object.
        ''' </summary>
        ''' <value>The object.</value>
        Public ReadOnly Property DataObject() As ormBusinessObject
            Get
                Return _relationEventArgs.Dataobject
            End Get
        End Property

    End Class
End Namespace

