
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

    Partial Public MustInherit Class ormDataObject


        ''' <summary>
        ''' Gets the GUID for the Object.
        ''' </summary>
        ''' <value>T</value>
        Public ReadOnly Property Guid() As Guid Implements iormPersistable.GUID
            Get
                Return Me._guid
            End Get
        End Property
        ''' <summary>
        ''' Sets the flag for ignoring the domainentry (delete on domain level)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsDomainIgnored As Boolean
            Get
                Return _DomainIsIgnored
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNIsDomainIgnored, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Gets the table store.
        ''' </summary>
        ''' <value>The table store.</value>
        Public ReadOnly Property PrimaryTableStore() As iormDataStore Implements iormPersistable.PrimaryTableStore
            Get
                If _record IsNot Nothing AndAlso _record.Alive AndAlso _record.TableStores IsNot Nothing Then
                    Return _record.GetTablestore(Me.PrimaryTableID)
                    ''' assume about the tablestore to choose
                ElseIf Not Me.RunTimeOnly AndAlso Me.PrimaryTableID IsNot Nothing Then
                    If _defaultdbdriver IsNot Nothing Then Return _defaultdbdriver.GetTableStore(tableID:=Me.PrimaryTableID)
                    Return ot.GetTableStore(tableid:=Me.PrimaryTableID)
                Else
                    Return Nothing
                End If
            End Get
        End Property
        ''' <summary>
        ''' True if a memory data object
        ''' </summary>
        ''' <value>The run time only.</value>
        Public ReadOnly Property RunTimeOnly() As Boolean Implements iormPersistable.RuntimeOnly
            Get
                Return Me._RunTimeOnly
            End Get
        End Property

        ''' <summary>
        ''' returns the object definition associated with this data object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectDefinition As ObjectDefinition Implements iormPersistable.ObjectDefinition
            Get

                If _objectdefinition Is Nothing Then
                    _RunTimeOnly = CurrentSession.IsBootstrappingInstallationRequested
                    _objectdefinition = CurrentSession.Objects.GetObject(objectid:=Me.ObjectID, runtimeOnly:=_RunTimeOnly)
                End If

                Return _objectdefinition

            End Get
        End Property
        ''' <summary>
        ''' returns the object class description associated with this data object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectClassDescription As ObjectClassDescription Implements iormPersistable.ObjectClassDescription
            Get
                If _classDescription Is Nothing Then
                    _classDescription = ot.GetObjectClassDescription(Me.GetType)
                End If
                Return _classDescription
            End Get

        End Property
        ''' <summary>
        ''' returns the tableschema associated with this data object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TableSchema() As iotDataSchema
            Get
                If Me.PrimaryTableStore IsNot Nothing Then
                    Return Me.PrimaryTableStore.TableSchema
                Else
                    Return Nothing
                End If

            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the domain ID.
        ''' </summary>
        ''' <value>The domain ID.</value>
        Public Overridable Property DomainID() As String
            Get
                If Me.ObjectHasDomainBehavior Then
                    Return Me._domainID
                Else
                    Return CurrentSession.CurrentDomainID
                End If
            End Get
            Set(value As String)
                SetValue(ConstFNDomainID, value)
            End Set
        End Property
        ''' <summary>
        '''  gets the DBDriver for the data object to use (real or the default dbdriver)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property DatabaseDriver As iormDatabaseDriver Implements iormPersistable.DatabaseDriver
            Get
                If Me.PrimaryTableStore IsNot Nothing Then Return Me.PrimaryTableStore.Connection.DatabaseDriver
                Return _defaultdbdriver
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the PS is initialized.
        ''' </summary>
        ''' <value>The PS is initialized.</value>
        Public ReadOnly Property IsInitialized() As Boolean Implements iormPersistable.IsInitialized
            Get
                Return Me._IsInitialized
            End Get

        End Property
        ''' <summary>
        ''' returns the ObjectID of the Class of this instance
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectID() As String Implements iormPersistable.ObjectID
            Get

                If Me.ObjectClassDescription IsNot Nothing Then
                    Return Me.ObjectClassDescription.ID
                Else
                    CoreMessageHandler("object id for orm data object class could not be found", arg1:=Me.GetType.Name, _
                                        subname:="ormDataObejct.ObjectID", messagetype:=otCoreMessageType.InternalError)
                End If
                Return Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the isDeleted.
        ''' </summary>
        ''' <value>The isDeleted.</value>
        Public Property IsDeleted() As Boolean
            Get
                Return Me._IsDeleted
            End Get
            Protected Friend Set(value As Boolean)
                Me._IsDeleted = value
                If value = False Then
                    _deletedOn = Nothing
                End If
            End Set
        End Property

        ''' <summary>
        ''' returns true if object has domain behavior
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectHasDomainBehavior As Boolean Implements iormPersistable.ObjectHasDomainBehavior
            Get
                ' do not initialize
                'If Not _IsInitialized AndAlso Not Initialize() Then
                '    CoreMessageHandler(message:="could not initialize object", subname:="ormDataObject.HasDomainBehavior")
                '    Return False
                'End If

                '** to avoid recursion loops for bootstrapping objects during 
                '** startup of session check these out and look into description
                If CurrentSession.IsBootstrappingInstallationRequested _
                    OrElse ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID) Then
                    Dim anObjectDecsription As ObjectClassDescription = ot.GetObjectClassDescription(Me.ObjectID)
                    If anObjectDecsription IsNot Nothing Then
                        Return anObjectDecsription.ObjectAttribute.AddDomainBehavior
                    Else
                        Return False
                    End If
                Else
                    Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                    If aObjectDefinition IsNot Nothing Then Return aObjectDefinition.HasDomainBehavior
                    Return False
                End If

            End Get

        End Property
        ''' <summary>
        ''' returns true if object is cached
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property UseCache As Boolean Implements iormPersistable.useCache
            Get
                ' do not initialize
                'If Not _IsInitialized AndAlso Not Initialize() Then
                '    CoreMessageHandler(message:="could not initialize object", subname:="ormDataObject.UseCache")
                '    Return False
                'End If
                If _useCache.HasValue Then
                    Return _useCache
                Else
                    '** to avoid recursion loops for bootstrapping objects during 
                    '** startup of session check these out and look into description
                    If CurrentSession.IsBootstrappingInstallationRequested _
                        OrElse ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID) Then
                        Dim anObjectDecsription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(Me.ObjectID)
                        If anObjectDecsription IsNot Nothing AndAlso anObjectDecsription.ObjectAttribute.HasValueUseCache Then
                            _useCache = anObjectDecsription.ObjectAttribute.UseCache
                        Else
                            _useCache = False
                        End If
                    Else
                        Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                        If aObjectDefinition IsNot Nothing Then Return aObjectDefinition.UseCache
                        _useCache = False
                    End If

                    Return _useCache
                End If

            End Get

        End Property
        ''' <summary>
        ''' returns true if object has delete per flag behavior
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectHasDeletePerFlagBehavior As Boolean Implements iormPersistable.ObjectHasDeletePerFlagBehavior
            Get
                ' do not initialize
                'If Not _IsInitialized AndAlso Not Initialize() Then
                '    CoreMessageHandler(message:="could not initialize object", subname:="ormDataObject.HasDeletePerFlagBehavior")
                '    Return False
                'End If
                '** avoid loops while starting up with bootstraps or during installation
                If CurrentSession.IsBootstrappingInstallationRequested OrElse ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID) Then
                    Dim anObjectDescription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(id:=Me.ObjectID)
                    If anObjectDescription IsNot Nothing Then
                        Return anObjectDescription.ObjectAttribute.AddDeleteFieldBehavior
                    Else
                        Return False
                    End If
                Else
                    Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                    '** per flag
                    If aObjectDefinition IsNot Nothing Then Return aObjectDefinition.HasDeleteFieldBehavior
                End If

            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the changed property
        ''' </summary>
        ''' <value>The PS is changed.</value>
        Public Property IsChanged() As Boolean Implements iormPersistable.isChanged
            Get
                Return Me._IsChanged
            End Get
            Protected Friend Set(value As Boolean)
                Me._IsChanged = value
                _changeTimeStamp = DateTime.Now
            End Set
        End Property
        ''' <summary>
        ''' Gets the changed property time stamp
        ''' </summary>
        ''' <value>The PS is changed.</value>
        Public ReadOnly Property ChangeTimeStamp() As DateTime Implements iormPersistable.ChangeTimeStamp
            Get
                Return _changeTimeStamp
            End Get
        End Property
        ''' <summary>
        ''' True if the Object was instanced by Retrieve
        ''' </summary>
        ''' <value>The PS is loaded.</value>
        Public ReadOnly Property IsLoaded() As Boolean Implements iormPersistable.IsLoaded
            Get
                Return _isLoaded
            End Get

        End Property
        ''' <summary>
        ''' returns True if the Object is infused
        ''' </summary>
        ''' <value>The PS is created.</value>
        Public ReadOnly Property IsInfused() As Boolean Implements iormPersistable.IsInfused
            Get
                Return _isInfused
            End Get
        End Property
        ''' <summary>
        '''  returns True if the Object was Instanced by Create
        ''' </summary>
        ''' <value>The PS is created.</value>
        Public ReadOnly Property IsCreated() As Boolean Implements iormPersistable.IsCreated
            Get
                Return _isCreated
            End Get
        End Property
        ''' <summary>
        ''' unload the Dataobject from the datastore
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function Unload() As Boolean
            _isLoaded = False
        End Function
        ''' <summary>
        ''' Gets or sets the OTDB record.
        ''' </summary>
        ''' <value>The OTDB record.</value>
        Public Property Record() As ormRecord Implements iormPersistable.Record
            Get
                Return Me._record
            End Get
            Set(value As ormRecord)
                If _record Is Nothing Then
                    Me._record = value
                Else
                    MergeRecord(value)
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns the primaryKeyvalues
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PrimaryKeyValues As Object() Implements iormPersistable.PrimaryKeyValues
            Get

                If (_primaryKeyValues Is Nothing OrElse _primaryKeyValues.Length = 0) AndAlso Me.IsAlive(throwError:=False, subname:="PrimaryKeyValue") _
                    AndAlso _primarykeynames IsNot Nothing AndAlso _primarykeynames.Length > 0 Then

                    If _primaryKeyValues Is Nothing OrElse _primaryKeyValues.length <> _primarykeynames.Length Then
                        ReDim _primaryKeyValues(_primarykeynames.Length - 1)
                    End If
                    For i = 0 To _primarykeynames.Length - 1
                        If _primarykeynames(i) IsNot Nothing Then
                            _primaryKeyValues(i) = Me.GetValue(_primarykeynames(i))
                        End If
                    Next
                End If
                Return _primaryKeyValues
            End Get
        End Property
        Public Property LoadedFromHost() As Boolean
            Get
                LoadedFromHost = _IsloadedFromHost
            End Get
            Protected Friend Set(value As Boolean)
                _IsloadedFromHost = value
            End Set
        End Property
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SavedToHost() As Boolean
            Get
                SavedToHost = _IsSavedToHost
            End Get
            Protected Friend Set(value As Boolean)
                _IsSavedToHost = value
            End Set
        End Property
        '** set the serialize with HostApplication
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SerializeWithHostApplication() As Boolean
            Get
                SerializeWithHostApplication = _serializeWithHostApplication
            End Get
            Protected Friend Set(value As Boolean)
                If value Then
                    If isRegisteredAtHostApplication(Me.PrimaryTableID) Then
                        _serializeWithHostApplication = True
                    Else
                        _serializeWithHostApplication = registerHostApplicationFor(Me.PrimaryTableID, AllObjectSerialize:=False)
                    End If
                Else
                    _serializeWithHostApplication = False
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets the associated tableids of this object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property TableIDs As String() Implements iormPersistable.TableIDs
            Get
                ''' to avoid loops get the description here
                If _tableids.Count = 0 Then
                    Dim anObjectDescription As ObjectClassDescription = Me.ObjectClassDescription
                    If anObjectDescription IsNot Nothing Then _tableids = anObjectDescription.ObjectAttribute.Tablenames
                End If

                Return _tableids
            End Get
        End Property

        ''' <summary>
        ''' gets the TableID of the primary Table of this object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property PrimaryTableID() As String Implements iormPersistable.primaryTableID
            Get
                ''' to avoid loops get the description here
                If _primaryTableID = "" Then
                    Dim anObjectDescription As ObjectClassDescription = Me.ObjectClassDescription
                    If anObjectDescription IsNot Nothing Then _primaryTableID = anObjectDescription.ObjectAttribute.Tablenames.First
                End If

                Return _primaryTableID
            End Get
        End Property
        ''' <summary>
        ''' gets the Creation date in the persistence store
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property CreatedOn() As Date?
            Get
                CreatedOn = _createdOn
            End Get
        End Property
        ''' <summary>
        ''' gets the last update date in the persistence store
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property UpdatedOn() As Date?
            Get
                UpdatedOn = _updatedOn
            End Get
        End Property
        ''' <summary>
        ''' gets the deletion date in the persistence store
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property DeletedOn() As Date?
            Get
                DeletedOn = _deletedOn
            End Get
            Friend Set(value As Date?)
                SetValue(ConstFNDeletedOn, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the additional spare parameter num1
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_num1() As Double?
            Get
                Return _parameter_num1
            End Get
            Set(value As Double?)
                SetValue(ConstFNParamNum1, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the additional spare parameter num2
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_num2() As Double?
            Get
                Return _parameter_num2
            End Get
            Set(value As Double?)
                SetValue(ConstFNParamNum2, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the additional spare parameter num3
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Property parameter_num3() As Double?
            Get
                Return _parameter_num3
            End Get
            Set(value As Double?)
                SetValue(ConstFNParamNum3, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the additional spare parameter date1
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_date1() As Date?
            Get
                Return _parameter_date1
            End Get
            Set(value As Date?)
                SetValue(ConstFNParamDate1, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the additional spare parameter date2
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_date2() As Date?
            Get
                Return _parameter_date2
            End Get
            Set(value As Date?)
                SetValue(ConstFNParamDate2, value)
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the additional spare parameter date3
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_date3() As Date?
            Get
                Return _parameter_date3
            End Get
            Set(value As Date?)
                SetValue(ConstFNParamDate3, value)
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the additional spare parameter flag1
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_flag1() As Boolean?
            Get
                Return _parameter_flag1
            End Get
            Set(value As Boolean?)
                SetValue(ConstFNParamFlag1, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the additional spare parameter flag3
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_flag3() As Boolean?
            Get
                parameter_flag3 = _parameter_flag3
            End Get
            Set(value As Boolean?)
                SetValue(ConstFNParamFlag3, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the additional spare parameter flag2
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_flag2() As Boolean?
            Get
                Return _parameter_flag2
            End Get
            Set(value As Boolean?)
                SetValue(ConstFNParamFlag2, value)
            End Set
        End Property

        ''' <summary>
        '''  gets or sets the additional spare parameter
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_txt1() As String
            Get
                Return _parameter_txt1
            End Get
            Set(value As String)
                SetValue(ConstFNParamText1, value)
            End Set
        End Property
        ''' <summary>
        '''  gets or sets the additional spare parameter
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_txt2() As String
            Get
                Return _parameter_txt2
            End Get
            Set(value As String)
                SetValue(ConstFNParamText2, value)
            End Set
        End Property
        ''' <summary>
        '''  gets or sets the additional spare parameter
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_txt3() As String
            Get
                Return _parameter_txt3
            End Get
            Set(value As String)
                SetValue(ConstFNParamText3, value)
            End Set
        End Property

    End Class
End Namespace
