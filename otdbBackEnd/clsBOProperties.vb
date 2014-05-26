
REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** BUSINESS OBJECTs: Extensible Properties Classes 
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2013-08-08
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** TO DO Log:
REM ***********             -
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
Imports OnTrack.Commons

Namespace OnTrack.ObjectProperties
    ''' <summary>
    ''' Enumeration and other definitions
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    Public Enum otLinkType
        One2One = 1
    End Enum

    ''' <summary>
    ''' class to define a set of properties attachable to other business objects
    ''' </summary>
    ''' <remarks>
    ''' 
    ''' Design Principles:
    ''' 
    ''' 1. Property sets are stand-alone and must exist before a property can be created.
    ''' 
    ''' 2. Properties are added by creating themselves e.g. Property.Create(setid:= ...). It will be added automatically to the set
    ''' 
    ''' 3. On loading the set all the properties will be retrieved as well due to relation.
    ''' 
    ''' </remarks>
    <ormObject(id:=ObjectPropertySet.ConstObjectID, version:=1, adddomainbehavior:=True, adddeletefieldbehavior:=True, usecache:=True, _
        modulename:=ConstModuleProperties, Title:="Property section", description:="definition of a section of properties attachable to bussiness object")> _
    Public Class ObjectPropertySet
        Inherits ormDataObject

        Public Const ConstObjectID = "PropertySet"

        ''' <summary>
        ''' Table Definition
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTable(version:=1, usecache:=True)> Public Const constTableID = "TBLDEFOBJPROPERTYSETS"

        '** primary Keys
        <ormObjectEntry(typeid:=otDataType.Text, size:=50, primaryKeyOrdinal:=1, _
            properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty},
            XID:="OPS1", title:="Set ID", description:="ID of the property set")> Public Const ConstFNSetID = "SETID"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=2 _
         , useforeignkey:=otForeignKeyImplementation.NativeDatabase, defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormObjectEntry(typeid:=otDataType.Text, isnullable:=True, _
          XID:="OPS3", title:="Description", description:="description of the property section")> Public Const ConstFNDescription = "DESC"

        <ormObjectEntry(typeid:=otDataType.List, isnullable:=True, _
          XID:="OPS4", title:="Properties", description:="properties of the object property section")> Public Const ConstFNProperties = "PROPERTIES"

        <ormObjectEntry(typeid:=otDataType.List, isnullable:=True, _
         XID:="OPS5", title:="Business Objects", description:="applicable business objects for this section")> Public Const ConstFNObjects = "OBJECTS"

        <ormObjectEntry(typeid:=otDataType.Long, defaultvalue:=1, dbdefaultvalue:="1", _
                        XID:="OPS6", title:="Ordinal", Description:="ordinal of the set")> Public Const ConstFNordinal As String = "ORDINAL"

        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>

        <ormEntryMapping(EntryName:=ConstFNSetID)> Private _id As String = ""
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNProperties)> Private _propertyids As New List(Of String)
        <ormEntryMapping(EntryName:=ConstFNObjects)> Private _objectids As New List(Of String)
        <ormEntryMapping(EntryName:=ConstFNordinal)> Private _ordinal As Long = 1
        ''' <summary>
        ''' Relations
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkobject:=GetType(ObjectProperty), cascadeOnDelete:=True, cascadeOnUpdate:=True, _
            fromEntries:={ConstFNSetID}, toEntries:={ObjectProperty.ConstFNSetID})> Public Const ConstRProperties = "PROPERTIES"

        <ormEntryMapping(RelationName:=ConstRProperties, infuseMode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand, _
            keyentries:={ObjectProperty.ConstFNPropertyID})> Private WithEvents _propertiesCollection As New ormRelationCollection(Of ObjectProperty)(Me, {ObjectProperty.ConstFNPropertyID})

#Region "Properties"

        ''' <summary>
        ''' Gets or sets the ordinal.
        ''' </summary>
        ''' <value>The ordinal.</value>
        Public Property Ordinal() As Long
            Get
                Return Me._ordinal
            End Get
            Set
                SetValue(ConstFNordinal, Value)
            End Set
        End Property

        '' <summary>
        ''' Gets or sets the attached object ids where this object propert set fits.
        ''' </summary>
        ''' <value>The properties.</value>
        Public Property AttachedObjectIDs() As List(Of String)
            Get
                Return Me._objectids
            End Get
            Set(value As List(Of String))
                SetValue(ConstFNObjects, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the properties ids.
        ''' </summary>
        ''' <value>The properties.</value>
        Public ReadOnly Property PropertyIDs() As List(Of String)
            Get
                Return Me._propertyids
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property Description() As String
            Get
                Return Me._description
            End Get
            Set(value As String)
                SetValue(ConstFNDescription, value)
            End Set
        End Property

        ''' <summary>
        ''' returns the ID of the configuration
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ID()
            Get
                Return _id
            End Get

        End Property

        ''' <summary>
        ''' returns the collection of Properties in this set
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Properties As ormRelationCollection(Of ObjectProperty)
            Get
                Return _propertiesCollection
            End Get
        End Property

#End Region

        ''' <summary>
        ''' retrieve  the property section from store
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="domainid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(id As String, Optional domainid As String = "") As ObjectPropertySet
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.Retrieve(Of ObjectPropertySet)(pkArray:={id.ToUpper, domainid.ToUpper}, domainID:=domainid)
        End Function

        ''' <summary>
        ''' creates a persistable property section
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="domainid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(id As String, Optional domainid As String = "") As ObjectPropertySet
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.CreateDataObject(Of ObjectPropertySet)(pkArray:={id.ToUpper, domainid.ToUpper}, domainID:=domainid, checkUnique:=True)
        End Function


        ''' <summary>
        ''' Handler for the OnAdded event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>

        Private Sub PropertiesCollection_OnAdded(sender As Object, e As Database.ormRelationCollection(Of ObjectProperty).EventArgs) Handles _propertiesCollection.OnAdded
            If Not _propertyids.Contains(e.Dataobject.ID) Then
                _propertyids.Add(e.Dataobject.ID)
            End If
        End Sub


    End Class

    ''' <summary>
    ''' class to define a configuration entity as member of a configuration attachable to other business objects
    ''' </summary>
    ''' <remarks>
    ''' 
    ''' Design principles:
    ''' 
    ''' 1. Properties can be created by Created -> will be added to the set by the property itself. If set doesnot exist also the property will not create
    ''' 
    ''' 2. the Class Property PropertySet is the cached backlink to the Set ( will not be loaded on infuse -> creates loops)
    ''' 
    ''' </remarks>
    <ormObject(id:=ObjectProperty.ConstObjectID, version:=1, adddomainbehavior:=True, adddeletefieldbehavior:=True, usecache:=True, _
        modulename:=ConstModuleProperties, Title:="property definition", description:="definition of a property attachable to business objects")> _
    Public Class ObjectProperty
        Inherits ObjectCompoundEntry

        Public Const ConstObjectID = "OBJECTPROPERTY"

        ''' <summary>
        ''' Table Definition
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTable(version:=1, usecache:=True)> Public Const ConstTableID = "TBLDEFOBJPROPERTY"

        ''' <summary>
        ''' Index 
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaIndex(columnname1:=ConstFNPropertyID, columnname2:=ConstFNSetID, columnname3:=ConstFNIsDeleted)> Public Const ConstINProperty = "INDEXPROPERTYIDS"
        <ormSchemaIndex(columnname1:=ConstFNObjectName, columnname2:=ConstFNType, columnname3:=ConstFNIsDeleted, columnname4:=ConstFNEntryName, enabled:=False)> Public Const constINDtypes = "indexTypes"

        ''' <summary>
        ''' primary keys
        ''' </summary>
        ''' <remarks></remarks>

        <ormObjectEntry(referenceObjectEntry:=ObjectPropertySet.ConstObjectID & "." & ObjectPropertySet.ConstFNSetID, primarykeyordinal:=1, _
            lookupPropertyStrings:={LookupProperty.UseForeignKey & "(" & constFKSet & ")"}, _
            validationPropertyStrings:={ObjectValidationProperty.NotEmpty, ObjectValidationProperty.UseLookup})> Public Const ConstFNSetID = ObjectPropertySet.ConstFNSetID

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, primaryKeyOrdinal:=2, _
            properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty},
            XID:="OPR2", title:="Name", description:="ID of the property")> Public Const ConstFNPropertyID = "PROPERTYID"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=3 _
         , useforeignkey:=otForeignKeyImplementation.None, defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormSchemaForeignKey(entrynames:={ConstFNSetID, ConstFNDomainID}, _
            foreignkeyreferences:={ObjectPropertySet.ConstObjectID & "." & ObjectPropertySet.ConstFNSetID, ObjectPropertySet.ConstObjectID & "." & ObjectPropertySet.ConstFNDomainID}, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFKSet = "FK_ObjPropertySet"

        ''' <summary>
        ''' other fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.List, isnullable:=True, _
          XID:="OPR4", title:="Extended Properties", description:="internal properties of the object property")> Public Shadows Const ConstFNExtProperties = "EXTPROPERTIES"

        ''' <summary>
        ''' Shadows with own XID
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
       

        ''' <summary>
        ''' disabled the inherited fields
        ''' </summary>
        ''' <remarks> 
        ''' this is only disabled if the value is exactly the same as inherited, since
        ''' the field value is taken as id/entryname of the entry and stored but the name of the constant is only used
        ''' for inheritage
        ''' </remarks>
        <ormObjectEntry(enabled:=False)> Public Const ConstFNObjectName As String = AbstractEntryDefinition.ConstFNObjectName
        <ormObjectEntry(enabled:=False)> Public Const ConstFNEntryName As String = AbstractEntryDefinition.ConstFNEntryName

        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>

        <ormEntryMapping(EntryName:=ConstFNSetID)> Private _setid As String = ""
        <ormEntryMapping(entryname:=ConstFNPropertyID)> Private _ID As String = ""
        <ormEntryMapping(EntryName:=ConstFNExtProperties)> Private _extproperties As String()

        ''' <summary>
        '''  further dynamic 
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        Private _relationpath As String() = {ObjectPropertyLink.ConstObjectID & "." & ObjectPropertyLink.ConstRPropertyValueLot, _
                                         ObjectPropertyValueLot.ConstObjectID & "." & ObjectPropertyValueLot.ConstRValues, _
                                         ObjectPropertyValue.ConstObjectID}
        Private _set As ObjectPropertySet 'cached

        Public Sub New()
            MyBase.New()
            MyBase.deregisterHandler() ' deregister the derived abstractentry handlers !
            AddHandler ormDataObject.OnCreating, AddressOf ObjectProperty_OnCreating
            AddHandler ormDataObject.OnCreated, AddressOf ObjectProperty_OnCreated
            AddHandler ormDataObject.OnInfused, AddressOf ObjectProperty_OnInfused
        End Sub

#Region "Properties"
        ''' <summary>
        ''' Gets the entity ID.
        ''' </summary>
        ''' <value>The entity.</value>
        Public ReadOnly Property ID() As String
            Get
                Return Me._ID
            End Get
        End Property

        ''' <summary>
        ''' returns the ID of the section
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property SetID As String
            Get
                Return _setid
            End Get
        End Property
        ''' <summary>
        ''' returns the property set
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PropertySet As ObjectPropertySet
            Get

                If _set Is Nothing Then
                    _set = ObjectPropertySet.Retrieve(id:=_setid)
                    If _set Is Nothing Then
                        CoreMessageHandler(message:="object property set does not exist", subname:="ObjectProperty.PropertySet", _
                                           messagetype:=otCoreMessageType.ApplicationError, _
                                           arg1:=_setid)
                        Return Nothing
                    End If
                End If
                Return _set
            End Get
        End Property


        ''' <summary>
        ''' Gets or sets the properties of the object property definition.
        ''' </summary>
        ''' <value>The properties.</value>
        Public Property ExtendedProperties() As String()
            Get
                Return Me._extproperties
            End Get
            Set(value As String())
                Me._extproperties = value
            End Set
        End Property


#End Region


        ''' <summary>
        ''' Handles OnCreating 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub ObjectProperty_OnCreating(sender As Object, e As ormDataObjectEventArgs)
            Dim my As ObjectProperty = TryCast(e.DataObject, ObjectProperty)

            If my IsNot Nothing Then
                Dim setid As String = e.Record.GetValue(ConstFNSetID)
                If setid Is Nothing Then
                    CoreMessageHandler(message:="object propert set doesnot exist", subname:="ObjectProperty.OnCreating", _
                                       messagetype:=otCoreMessageType.ApplicationError, _
                                       arg1:=my.SetID)
                    e.AbortOperation = True
                    Return
                End If
                ''' even if it is early to retrieve the set and set it (since this might disposed since we have not run through checkuniqueness and cache)
                ''' we need to check on the object here
                _set = ObjectPropertySet.Retrieve(id:=setid)
                If _set Is Nothing Then
                    CoreMessageHandler(message:="object propert set doesnot exist", subname:="ObjectProperty.OnCreated", _
                                       messagetype:=otCoreMessageType.ApplicationError, _
                                       arg1:=my.SetID)
                    e.AbortOperation = True
                    Return
                End If
            End If
        End Sub

        ''' <summary>
        ''' Handles OnCreated and Relation to ConfigSet
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub ObjectProperty_OnCreated(sender As Object, e As ormDataObjectEventArgs)
            Dim my As ObjectProperty = TryCast(e.DataObject, ObjectProperty)

            If my IsNot Nothing Then
                If _set Is Nothing Then
                    _set = ObjectPropertySet.Retrieve(id:=SetID)
                    If _set Is Nothing Then
                        CoreMessageHandler(message:="object propert set doesnot exist", subname:="ObjectProperty.OnCreated", _
                                          messagetype:=otCoreMessageType.ApplicationError, _
                                           arg1:=my.SetID)
                        e.AbortOperation = True
                        Return
                    End If
                End If
            End If

        End Sub
        ''' <summary>
        ''' Handles OnCreating and Relation to ConfigSection
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub ObjectProperty_OnInfused(sender As Object, e As ormDataObjectEventArgs)
            Dim my As ObjectProperty = TryCast(e.DataObject, ObjectProperty)

            ''' infuse is called on create as well as on retrieve / inject 
            ''' only on the create case we need to add to the properties otherwise
            ''' propertyset will load the property
            ''' or the property will stand alone
            If my IsNot Nothing AndAlso e.Infusemode = otInfuseMode.OnCreate AndAlso _set IsNot Nothing Then
                _set.Properties.Add(my)
            End If
        End Sub
        ''' <summary>
        ''' set the values of a compound from a property
        ''' </summary>
        ''' <param name="compound"></param>
        ''' <param name="property"></param>
        ''' <remarks></remarks>
        Private Sub SetCompound(compound As ObjectCompoundEntry)
            ''' set the values
            ''' 
            With compound
                '' type and field
                .Aliases = Me.Aliases
                .Datatype = Me.Datatype
                .IsNullable = Me.IsNullable
                .DefaultValue = Nothing
                .Size = Me.Size
                .InnerDatatype = Me.InnerDatatype
                .Version = Me.Version
                .Title = Me.Title
                .Description = Me.Description
                ' ordinal calculate an ordinal
                .Ordinal = 1000 + (Me.PropertySet.Ordinal - 1) * 100 + Me.Ordinal
                ' addition
                .LookupCondition = Me.LookupCondition
                .LookupProperties = Me.LookupProperties
                .PossibleValues = Me.PossibleValues
                .LowerRangeValue = Me.LowerRangeValue
                .UpperRangeValue = Me.UpperRangeValue
                .ValidateRegExpression = Me.ValidateRegExpression
                .Validationproperties = Me.Validationproperties
                .XID = Me.XID
                If .XID Is Nothing Then .XID = Me.SetID & "." & Me.ID
                .IsValidating = Me.IsValidating
                .RenderProperties = Me.RenderProperties
                .RenderRegExpMatch = Me.RenderRegExpMatch
                .RenderRegExpPattern = Me.RenderRegExpMatch
                .IsRendering = Me.IsRendering

                ''' special compound settings
                .CompoundObjectID = ObjectPropertyValue.ConstObjectID
                .CompoundValueEntryName = ObjectPropertyValue.ConstFNValue
                .CompoundIDEntryname = ObjectPropertyValue.ConstFNPropertyID
                .CompoundSetterMethodName = Nothing
                .CompoundGetterMethodName = Nothing
                .CompoundRelationPath = {}

            End With
        End Sub
        ''' <summary>
        ''' OnPersisted Handler to add the Properties as Compounds to the ObjectIDs
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ObjectProperty_OnPersisted(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnPersisted

            ''' attach the Properties as compounds
            ''' 
            Dim aSet = Me.PropertySet
            If aSet Is Nothing Then
                CoreMessageHandler(message:="object propert set doesnot exist", subname:="ObjectProperty.OnPersisted", _
                                   messagetype:=otCoreMessageType.ApplicationError, _
                                   arg1:=Me.SetID)
                e.AbortOperation = True
                Return
            End If

            For Each anObjectID In aSet.AttachedObjectIDs
                Dim anObjectDefinition As ObjectDefinition = CurrentSession.Objects.GetObject(objectid:=anObjectID)
                If anObjectDefinition IsNot Nothing Then
                    Dim apath As String()
                    ReDim apath(_relationpath.GetUpperBound(0) + 1)
                    ''' set it to the linking objects
                    ''' 
                    If anObjectDefinition.ID = Deliverable.ConstObjectID Then
                        apath(0) = anObjectID & "." & Deliverable.ConstRPropertyLink
                    ElseIf ObjectDefinition.ID <> "" Then
                        CoreMessageHandler(message:="other objects for properties to be linked to not implemented", subname:="ObjectPropertySet.OnPersisted", _
                                            arg1:=anObjectDefinition.ID, objectname:=Me.ObjectID)
                    End If

                    Array.ConstrainedCopy(_relationpath, 0, apath, 1, apath.Length - 1)

                    ''' create all the relational path
                    ''' 
                    For i = apath.GetLowerBound(0) To apath.GetUpperBound(0) - 1
                        Dim names As String() = Shuffle.NameSplitter(apath(i)) ' get the objectname from the canonical form
                        Dim aCompound As ObjectCompoundEntry = ObjectCompoundEntry.Create(objectname:=names(0), _
                                                                                     entryname:=Me.ID, domainID:=Me.DomainID, _
                                                                                     runtimeOnly:=Me.RunTimeOnly, checkunique:=True)
                        If aCompound Is Nothing Then aCompound = ObjectCompoundEntry.Retrieve(objectname:=names(0), _
                                                                                     entryname:=Me.ID, runtimeOnly:=Me.RunTimeOnly)

                        ''' set the values
                        ''' 
                        SetCompound(compound:=aCompound)
                        Dim relpath As String()
                        ReDim relpath(apath.GetUpperBound(0) - i)
                        Array.ConstrainedCopy(apath, i, relpath, 0, relpath.Length)
                        aCompound.CompoundRelationPath = relpath

                        ''' on ObjectPropertyvLink Level we need to go to the setter to enable
                        ''' versioning on the lot if a changed property is needed
                        If names(0) = ObjectPropertyLink.ConstObjectID.ToUpper Then
                            aCompound.CompoundSetterMethodName = ObjectPropertyLink.ConstOPSetCompoundValue
                            ''' 
                            ''' on the end take the setter / getter operations to resolve
                            ''' 
                        ElseIf names(0) = ObjectPropertyValueLot.ConstObjectID.ToUpper Then
                            aCompound.CompoundSetterMethodName = ObjectPropertyValueLot.ConstOPSetCompoundValue
                            aCompound.CompoundGetterMethodName = ObjectPropertyValueLot.ConstOPGetCompoundValue
                        End If
                ''' set it to the linking objects
                ''' 

                aCompound.Persist()

            Next


                End If
            Next
        End Sub
        ''' <summary>
        ''' OnDeleted Handler to add the Properties as Compounds to the ObjectIDs
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ObjectProperty_OnDeleted(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnDeleted

            ''' attach the Properties as compounds
            ''' 
            For Each anObjectID In _set.AttachedObjectIDs
                Dim anObjectDefinition As ObjectDefinition = CurrentSession.Objects.GetObject(objectid:=anObjectID)
                If anObjectDefinition IsNot Nothing Then
                    Dim apath As String()
                    ReDim apath(_relationpath.GetUpperBound(0) + 1)
                    apath(0) = anObjectID
                    Array.ConstrainedCopy(_relationpath, 0, apath, 1, apath.Length)
                    ''' create all the relational path
                    ''' 
                    For i = apath.GetUpperBound(0) To apath.GetUpperBound(0) - 1
                        Dim aCompound As ObjectCompoundEntry = ObjectCompoundEntry.Retrieve(apath(i), Me.ID, runtimeOnly:=Me.RunTimeOnly)
                        If aCompound IsNot Nothing Then aCompound.Delete()
                    Next

                End If
            Next
        End Sub
        ''' <summary>
        ''' create a persistable ObjectProperty
        ''' </summary>
        ''' <param name="Section"></param>
        ''' <param name="Entity"></param>
        ''' <param name="domainid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(setid As String, ID As String, Optional domainid As String = "") As ObjectProperty
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim primarykey As Object() = {setid.ToUpper, ID.ToUpper, domainid}
            Return ormDataObject.CreateDataObject(Of ObjectProperty)(pkArray:=primarykey, domainID:=domainid, checkUnique:=True)
        End Function

        ''' <summary>
        ''' create a persistable ObjectProperty
        ''' </summary>
        ''' <param name="Section"></param>
        ''' <param name="Entity"></param>
        ''' <param name="domainid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(setid As String, ID As String, Optional domainid As String = "") As ObjectProperty
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim primarykey As Object() = {setid.ToUpper, ID.ToUpper, domainid}
            Return ormDataObject.Retrieve(Of ObjectProperty)(pkArray:=primarykey)
        End Function
    End Class

'    SELECT      TBLOBJPROPERTYLINKS.FROMOBJECTID, TBLOBJPROPERTYLINKS.fromuid, tblobjpropertylinks.FROMUPDC ,
'		    TBLOBJPROPERTYLINKS.TOUID, TBLOBJPROPERTYLINKS.toupdc, LOT.PUID, LOT.UPDC, P1.VALUE as '0.0.2.0',P2.value AS '0.0.3.0' , P3.VALUE AS '0.1.0.0', P4.VALUE AS '0.1.3.0', 
'              P5.VALUE AS '0.1.6.0', P6.VALUE AS '0.2.0.0',  P7.VALUE AS '0.2.3.0', P8.VALUE AS '0.2.6.0', P9.VALUE AS '0.3.0.0',
'			   P10.VALUE AS '0.4.0.0',  P11.VALUE AS '0.5.0.0',  P12.VALUE AS '0.6.0.0',  P13.VALUE AS '1.0.0.0'
'FROM            ontrack.dbo.TBLOBJPROPERTYVALUELOTS AS LOT 
' INNER JOIN               ontrack.dbo.TBLOBJPROPERTYVALUES AS P1 ON LOT.PUID = P1.PUID AND LOT.UPDC = P1.UPDC AND P1.PROPERTYID = '0.0.2.0'
' INNER JOIN				ontrack.dbo.TBLOBJPROPERTYVALUES AS P2 ON LOT.PUID = P2.PUID AND LOT.UPDC = P2.UPDC AND P2.PROPERTYID = '0.0.3.0'
' INNER JOIN				ontrack.dbo.TBLOBJPROPERTYVALUES AS P3 ON LOT.PUID = P3.PUID AND LOT.UPDC = P3.UPDC AND P3.PROPERTYID = '0.1.0.0'
'INNER JOIN				ontrack.dbo.TBLOBJPROPERTYVALUES AS P4 ON LOT.PUID = P4.PUID AND LOT.UPDC = P4.UPDC AND P4.PROPERTYID = '0.1.3.0'
'INNER JOIN				ontrack.dbo.TBLOBJPROPERTYVALUES AS P5 ON LOT.PUID = P5.PUID AND LOT.UPDC = P5.UPDC AND P5.PROPERTYID = '0.1.6.0'
'INNER JOIN				ontrack.dbo.TBLOBJPROPERTYVALUES AS P6 ON LOT.PUID = P6.PUID AND LOT.UPDC = P6.UPDC AND P6.PROPERTYID = '0.2.0.0'
'INNER JOIN				ontrack.dbo.TBLOBJPROPERTYVALUES AS P7 ON LOT.PUID = P7.PUID AND LOT.UPDC = P7.UPDC AND P7.PROPERTYID = '0.2.3.0'
'INNER JOIN				ontrack.dbo.TBLOBJPROPERTYVALUES AS P8 ON LOT.PUID = P8.PUID AND LOT.UPDC = P8.UPDC AND P8.PROPERTYID = '0.2.6.0'
'INNER JOIN				ontrack.dbo.TBLOBJPROPERTYVALUES AS P9 ON LOT.PUID = P9.PUID AND LOT.UPDC = P9.UPDC AND P9.PROPERTYID = '0.3.0.0'
'INNER JOIN				ontrack.dbo.TBLOBJPROPERTYVALUES AS P10 ON LOT.PUID = P10.PUID AND LOT.UPDC = P10.UPDC AND P10.PROPERTYID = '0.4.0.0'
'INNER JOIN				ontrack.dbo.TBLOBJPROPERTYVALUES AS P11 ON LOT.PUID = P11.PUID AND LOT.UPDC = P11.UPDC AND P11.PROPERTYID = '0.5.0.0'
'INNER JOIN				ontrack.dbo.TBLOBJPROPERTYVALUES AS P12 ON LOT.PUID = P12.PUID AND LOT.UPDC = P12.UPDC AND P12.PROPERTYID = '0.6.0.0'
'INNER JOIN				ontrack.dbo.TBLOBJPROPERTYVALUES AS P13 ON LOT.PUID = P13.PUID AND LOT.UPDC = P13.UPDC AND P13.PROPERTYID = '1.0.0.0'
'inner join	ontrack.dbo.TBLOBJPROPERTYLINKS on lot.puid = TBLOBJPROPERTYLINKS.touid 
    ''' <summary>
    ''' the Property LINK class links a busines object to a value collection
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=ObjectPropertyLink.ConstObjectID, modulename:=ConstModuleProperties, Version:=1, _
        usecache:=True, adddomainbehavior:=True, adddeletefieldbehavior:=True, _
        description:="link definitions between properties via value collection and other business objects")> _
    Public Class ObjectPropertyLink
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "PropertyLink"

        '** Schema Table
        <ormSchemaTable(version:=1)> Public Const ConstTableID = "TBLOBJPROPERTYLINKS"

        '** index
        <ormSchemaIndex(columnname1:=ConstFNToUid, columnname2:=ConstFNFromObjectID, columnname3:=ConstFNFromUid)> Public Const ConstIndTag = "used"

        ''' <summary>
        ''' Primary key of the CONFIG link object
        ''' FROM an ObjectID, UID, UPDC (KEY)
        ''' TO   an OBJECTID, UID, UPDC
        ''' 
        ''' links a  business objects (deliverable, pars, configcondition (for own use) ) with a property set
        ''' also capable of linking schedules to schedules or milestones of schedules to schedules
        ''' 
        ''' </summary>
        ''' <remarks></remarks>
        ''' 

        <ormObjectEntry(referenceobjectentry:=ObjectDefinition.ConstObjectID & "." & ObjectDefinition.ConstFNID, primarykeyordinal:=1, _
            properties:={ObjectEntryProperty.Keyword}, _
            validationPropertyStrings:={ObjectValidationProperty.NotEmpty, ObjectValidationProperty.UseLookup}, _
            LookupPropertyStrings:={LookupProperty.UseAttributeValues}, _
            values:={Deliverable.ConstObjectID, Parts.Part.ConstObjectID, Configurables.ConfigItemSelector.ConstObjectID}, _
            dbdefaultvalue:=Deliverable.ConstObjectID, defaultvalue:=Deliverable.ConstObjectID, _
            XID:="OPL1", title:="From Object", description:="from object id of the business object")> _
        Public Const ConstFNFromObjectID = "FROMOBJECTID"

        <ormObjectEntry(typeid:=otDataType.Long, primarykeyordinal:=2, dbdefaultvalue:="0", lowerrange:=0, _
            XID:="OPL2", title:="Linked from UID", description:="from uid of the business object")> _
        Public Const ConstFNFromUid = "FROMUID"

        <ormObjectEntry(typeid:=otDataType.Long, primarykeyordinal:=3, dbdefaultvalue:="0", lowerrange:=0, _
            XID:="OPL3", title:="Linked from UPDC", description:="from uid of the business object")> _
        Public Const ConstFNFromUpdc = "FROMUPDC"

        ' deactivate ForeignKEy we do not have this object in domains
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=4, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNDomainID = Domain.ConstFNDomainID


        ''' <summary>
        ''' Column Definitions
        ''' </summary>
        ''' <remarks></remarks>

        <ormObjectEntry(typeid:=otDataType.Long, dbdefaultvalue:="0", lowerrange:=0, _
            XID:="OPL5", title:="Linked to UID", description:="uid link to the property value lot object")> _
        Public Const ConstFNToUid = "TOUID"

        <ormObjectEntry(typeid:=otDataType.Long, isnullable:=True, lowerrange:=0, _
            XID:="OPL6", title:="Linked to UPDC", description:="updc link to the property value lot object")> _
        Public Const ConstFNToUpdc = "TOUPDC"

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, dbdefaultvalue:="One2One", defaultvalue:=otLinkType.One2One, _
            XID:="OPL10", title:="Linke Type", description:="object link type")> Public Const ConstFNTypeID = "typeid"

        ''' <summary>
        ''' Mappings persistable members
        ''' </summary>
        ''' <remarks></remarks>
        <ormEntryMapping(EntryName:=ConstFNFromObjectID)> Private _FromObjectID As String
        <ormEntryMapping(EntryName:=ConstFNFromUid)> Private _FromUid As Long
        <ormEntryMapping(EntryName:=ConstFNFromUpdc)> Private _FromUpdc As Long

        <ormEntryMapping(EntryName:=ConstFNToUid)> Private _ToUid As Long
        <ormEntryMapping(EntryName:=ConstFNToUpdc)> Private _ToUpdc As Long
        <ormEntryMapping(EntryName:=ConstFNTypeID)> Private _type As otLinkType

        ''' <summary>
        ''' Relation to PropertyValueLot - will be resolved by event handler on relation manager
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(ObjectPropertyValueLot), createobjectifnotretrieved:=True, toPrimarykeys:={ConstFNToUid, ConstFNToUpdc}, _
                     cascadeonCreate:=True, cascadeOnDelete:=True, cascadeOnUpdate:=True)> _
        Public Const ConstRPropertyValueLot = "RELPROPERTYVALUELOT"

        <ormEntryMapping(relationName:=ConstRPropertyValueLot, infusemode:=otInfuseMode.OnCreate Or otInfuseMode.OnInject Or otInfuseMode.OnDemand)> _
        Private _propertyValueLot As ObjectPropertyValueLot

        ''' <summary>
        ''' Define the constants for accessing the compounds
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstOPGetCompoundValue = "GETPROPERTYVALUE"
        Public Const ConstOPSetCompoundValue = "SETPROPERTYVALUE"

        ''' <summary>
        ''' dynamic data
        ''' </summary>
        ''' <remarks></remarks>
        Private _prevVersionLots As New List(Of ObjectPropertyValueLot) 'list of previous versions we we issue a version change

#Region "properties"

        ''' <summary>
        ''' Gets or sets the property value lot.
        ''' </summary>
        ''' <value>The property value lot.</value>
        Public ReadOnly Property PropertyValueLot() As ObjectPropertyValueLot
            Get
                If Not IsAlive(subname:="PropertyValueLot") Then Return Nothing

                If Me.GetRelationStatus(ConstRPropertyValueLot) <> DataObjectRelationMgr.RelationStatus.Loaded Then
                    Me.InfuseRelation(ConstRPropertyValueLot)
                End If
                Return Me._propertyValueLot
            End Get

        End Property

        ''' <summary>
        ''' Gets or sets the type.
        ''' </summary>
        ''' <value>The type.</value>
        Public Property Type() As otLinkType
            Get
                Return Me._type
            End Get
            Set(value As otLinkType)
                Me._type = value
            End Set
        End Property

        ''' <summary>
        ''' gets the object id of the linking object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property FromObjectID() As String
            Get
                Return _FromObjectID
            End Get

        End Property
        ''' <summary>
        ''' gets the UID of the linking object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property FromUID() As Long
            Get
                Return _FromUid
            End Get

        End Property
        ''' <summary>
        ''' gets the Updc of the linking object - returns zero if not applicable
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property FromUPDC() As Long
            Get
                Return _FromUpdc
            End Get

        End Property

        ''' <summary>
        ''' gets or sets the UID of the linked object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ToUID() As Long
            Get
                Return _ToUid
            End Get
            Set(value As Long)
                SetValue(ConstFNToUid, value)
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the Updc of the linked object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ToUpdc() As Long?
            Get
                Return _ToUpdc
            End Get
            Set(value As Long?)
                SetValue(ConstFNToUpdc, value)
            End Set
        End Property
#End Region

        ''' <summary>
        ''' operation to set a PropertyValue - here we must change to next version (updc) of the 
        ''' </summary>
        ''' <param name="id">the property</param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ormObjectOperationMethod(operationname:=ConstOPSetCompoundValue, tag:=ObjectCompoundEntry.ConstCompoundSetter, _
            parameterEntries:={ObjectCompoundEntry.ConstFNEntryName, ObjectCompoundEntry.ConstFNValues})> _
        Public Function SetPropertyValue(id As String, value As Object) As Boolean
            If Not IsAlive(subname:="SetPropertyValue") Then Return False

            ''' get the relation
            ''' 
            If Me.PropertyValueLot Is Nothing Then
                Return False
            End If

            '''
            ''' check if the new Property value is different then old one
            ''' 
            If Not Me.PropertyValueLot.EqualsValue(id, value) Then
                ''' we need change the version of the properyvaluelot if we have not done so (then it is created)
                ''' 
                If Not Me.PropertyValueLot.IsCreated Then
                    Dim aNewLot As ObjectPropertyValueLot = Me.PropertyValueLot.Clone()
                    Me.ToUID = aNewLot.UID
                    Me.ToUpdc = aNewLot.UPDC ' set new one
                    Me.PropertyValueLot.ValidUntil = Date.Now
                    _prevVersionLots.Add(_propertyValueLot)
                    _propertyValueLot = aNewLot

                End If

                Return _propertyValueLot.SetValue(entryname:=id, value:=value)
            Else
                ''' nothing to do
                ''' 
                Return True
            End If

        End Function
        ''' <summary>
        ''' handles the onPersisted Event to save the previous versions
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ObjectPropertyLink_OnPersisted(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnPersisted
            For Each aLot In _prevVersionLots
                aLot.Persist()
            Next
        End Sub
        ''' <summary>
        ''' Event Handler for the RelationRetrieveNeeded event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ObjectPropertyLink_OnRelationRetrieveNeeded(sender As Object, e As ormDataObjectRelationEventArgs) Handles Me.OnRelationRetrieveNeeded
            If Not Me.IsAlive(subname:="ObjectPropertyLink_OnRelationRetrieveNeeded") Then Return
            ''' check on PropertyValueLot
            ''' 
            If e.RelationID = ConstRPropertyValueLot Then
                Dim aPropertyLot As ObjectPropertyValueLot = ObjectPropertyValueLot.Retrieve(uid:=Me.ToUID, updc:=Me.ToUpdc)
                e.RelationObjects.Add(aPropertyLot)
                e.Finished = True
            End If
        End Sub
        ''' <summary>
        ''' Event Handler for the RelationRetrieveNeeded event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ObjectPropertyLink_OnRelationCreateNeeded(sender As Object, e As ormDataObjectRelationEventArgs) Handles Me.OnRelationCreateNeeded
            If Not Me.IsAlive(subname:="Deliverable_OnRelationCreateNeeded") Then Return
            ''' check on PropertyValueLot
            ''' 
            If e.RelationID = ConstRPropertyValueLot Then
                Dim aPropertyLot As ObjectPropertyValueLot = ObjectPropertyValueLot.Create(uid:=Me.ToUID, updc:=Me.ToUpdc)
                If aPropertyLot Is Nothing Then aPropertyLot = ObjectPropertyValueLot.Retrieve(uid:=Me.ToUID, updc:=Me.ToUpdc)

                ' we have what we need
                e.RelationObjects.Add(aPropertyLot)
                e.Finished = True

            End If
        End Sub
        ''' <summary>
        ''' create a persitable link object
        ''' </summary>
        ''' <param name="fromid"></param>
        ''' <param name="fromuid"></param>
        ''' <param name="toid"></param>
        ''' <param name="touid"></param>
        ''' <param name="frommilestone"></param>
        ''' <param name="tomilestone"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(fromObjectID As String, _
                                                fromuid As Long, _
                                                Optional fromupdc As Long = 0, _
                                                Optional domainid As String = "", _
                                                Optional toUID As Long? = Nothing, _
                                                Optional toUpdc As Long? = Nothing) As ObjectPropertyLink
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim primarykey As Object() = {fromObjectID, fromuid, fromupdc, domainid}

            '' set values
            Dim aRecord As New ormRecord
            With aRecord
                .SetValue(ConstFNFromObjectID, fromObjectID)
                .SetValue(ConstFNFromUid, fromuid)
                .SetValue(ConstFNFromUpdc, fromupdc)
                .SetValue(ConstFNDomainID, domainid)
                '.SetValue(ConstFNToObjectID, ObjectPropertyValueLot.ConstObjectID)
                .SetValue(ConstFNToUid, toUID)
                .SetValue(ConstFNToUpdc, toUpdc)
            End With

            Return ormDataObject.CreateDataObject(Of ObjectPropertyLink)(aRecord, checkUnique:=True)
        End Function

        ''' <summary>
        ''' retrieve a persitable link object
        ''' </summary>
        ''' <param name="fromid"></param>
        ''' <param name="fromuid"></param>
        ''' <param name="toid"></param>
        ''' <param name="touid"></param>
        ''' <param name="frommilestone"></param>
        ''' <param name="tomilestone"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(fromObjectID As String, fromUid As Long, fromUpdc As Long, Optional domainid As String = "") As ObjectPropertyLink
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim primarykey As Object() = {fromObjectID, fromUid, fromUpdc, domainid}
            Return ormDataObject.Retrieve(Of ObjectPropertyLink)(primarykey)
        End Function
    End Class

    ''' <summary>
    ''' class for a lot or set of object properties values  attached to other business objects
    ''' </summary>
    ''' <remarks>
    ''' Design Principles
    ''' 
    ''' 1. The Lot takes care of the values by the SetPropertyValue, GetPropertyValue Routine
    ''' 
    ''' 2. The Lot loads or creates with the AddSet Function all the Properties in its collection.
    ''' 
    ''' 3. setPropertyValue also issues an AddSet with new Sets to be assigned values to
    ''' </remarks>
    <ormObject(id:=ObjectPropertyValueLot.ConstObjectID, version:=1, adddomainbehavior:=False, adddeletefieldbehavior:=True, usecache:=True, _
        modulename:=ConstModuleProperties, Title:="Property Value Lot", description:="Lot of properties values attached to bussiness object")> _
    Public Class ObjectPropertyValueLot
        Inherits ormDataObject
        Implements iormCloneable(Of ObjectPropertyValueLot)


        Public Const ConstObjectID = "PropertyValueLot"

        ''' <summary>
        ''' Table Definition
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTable(version:=1, usecache:=False)> Public Const constTableID = "TBLOBJPROPERTYVALUELOTS"

        ''' <summary>
        ''' primary keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Long, primaryKeyOrdinal:=1, dbdefaultvalue:="0", _
              XID:="PLOT1", title:="Lot UID", description:="UID of the property value lot")> Public Const constFNUID = "PUID"

        <ormObjectEntry(typeid:=otDataType.Long, dbdefaultvalue:="0", primaryKeyordinal:=2, _
            title:="update count", Description:="Update count of the property value lot", XID:="PLOT2")> Public Const ConstFNUpdc = "UPDC"

        ''' <summary>
        ''' fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, defaultvalue:=ConstGlobalDomain, _
          useforeignkey:=otForeignKeyImplementation.None, dbdefaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormObjectEntry(typeid:=otDataType.Text, isnullable:=True, _
          XID:="PLOT3", title:="Description", description:="description of the property value lot")> Public Const ConstFNDescription = "DESC"

        <ormObjectEntry(typeid:=otDataType.List, _
         lookupPropertyStrings:={LookupProperty.UseObjectEntry & "(" & ObjectPropertySet.ConstObjectID & "." & ObjectPropertySet.ConstFNSetID & ")"}, validationPropertyStrings:={ObjectValidationProperty.UseLookup}, _
         XID:="PLOT4", title:="Property Sets", description:="applicable property sets for this lot")> Public Const ConstFNSets = "SETS"

        <ormObjectEntry(typeid:=otDataType.Date, isnullable:=True, _
        XID:="PLOT11", title:="valid from", description:="property set is valid from ")> Public Const ConstFNValidFrom = "validfrom"

        <ormObjectEntry(typeid:=otDataType.Date, isnullable:=True, _
       XID:="PLOT12", title:="valid until", description:="property set is valid until ")> Public Const ConstFNValiduntil = "validuntil"


        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>

        <ormEntryMapping(EntryName:=constFNUID)> Private _uid As Long = 0
        <ormEntryMapping(EntryName:=ConstFNUpdc)> Private _updc As Long = 0
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNSets)> Private _setids As String() = {}
        <ormEntryMapping(EntryName:=ConstFNValidFrom)> Private _validfrom As DateTime?
        <ormEntryMapping(EntryName:=ConstFNValiduntil)> Private _validuntil As DateTime?

        ''' <summary>
        ''' Relations of compound
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkobject:=GetType(ObjectPropertyValue), cascadeOnDelete:=True, cascadeOnUpdate:=True, _
            fromEntries:={constFNUID, ConstFNUpdc}, toEntries:={ObjectPropertyValue.constFNUID, ObjectPropertyValue.ConstFNUpdc})> Public Const ConstRValues = "RELVALUES"

        <ormEntryMapping(RelationName:=ConstRValues, infuseMode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand, _
            keyentries:={ObjectPropertyValue.ConstFNSetID, ObjectPropertyValue.ConstFNPropertyID})> _
        Private WithEvents _valuesCollection As New ormRelationCollection(Of ObjectPropertyValue)(Me, {ObjectPropertyValue.ConstFNSetID, ObjectPropertyValue.ConstFNPropertyID})

        ''' <summary>
        ''' Define the constants for accessing the compounds
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstOPGetCompoundValue = "GETPROPERTYVALUE"
        Public Const ConstOPSetCompoundValue = "SETPROPERTYVALUE"

        ''' <summary>
        ''' dynamic members
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        Private _changedPropertyValues As New Dictionary(Of String, ObjectPropertyValue)

#Region "Properties"

        ''' <summary>
        ''' Gets or sets the validto date.
        ''' </summary>
        ''' <value>The validto.</value>
        Public Property ValidUntil() As DateTime?
            Get
                Return Me._validuntil
            End Get
            Set(value As DateTime?)
                SetValue(ConstFNValiduntil, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the validfrom.
        ''' </summary>
        ''' <value>The validfrom.</value>
        Public Property Validfrom() As DateTime?
            Get
                Return Me._validfrom
            End Get
            Set(value As DateTime?)
                SetValue(ConstFNValidFrom, value)
            End Set
        End Property

        '' <summary>
        ''' Gets or sets the section id s.
        ''' </summary>
        ''' <value>The properties.</value>
        Public Property PropertySetIDs() As String()
            Get
                Return Me._setids
            End Get
            Set(value As String())
                SetValue(ConstFNSets, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property Description() As String
            Get
                Return Me._description
            End Get
            Set(value As String)
                SetValue(ConstFNDescription, value)
            End Set
        End Property

        ''' <summary>
        ''' returns the UID of the configuration set
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property UID() As Long
            Get
                Return _uid
            End Get
        End Property

        ''' <summary>
        ''' returns the UID of the configuration set
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property UPDC() As Long
            Get
                Return _updc
            End Get
        End Property
        ''' <summary>
        ''' returns the Entities of this Section
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Values As ormRelationCollection(Of ObjectPropertyValue)
            Get
                Return _valuesCollection
            End Get
        End Property

#End Region



        ''' <summary>
        ''' operation to Access the Compound's Value
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ormObjectOperationMethod(operationname:=ConstOPGetCompoundValue, tag:=ObjectCompoundEntry.ConstCompoundGetter, _
            parameterEntries:={ObjectCompoundEntry.ConstFNEntryName, ObjectCompoundEntry.ConstFNValues})> _
        Public Function GetPropertyValue(id As String, ByRef value As Object) As Boolean
            If Not IsAlive(subname:="GetPropertyValue") Then Return Nothing
            Dim propertyID As String = id.ToUpper

            ''' the id should be in a canonical form
            ''' 
            Dim names = Shuffle.NameSplitter(id)

            ''' if we have a set then check 
            If names.Count = 1 Then
                If _setids.Count = 0 Then
                    ''' we could look up if this is unqiue
                    ''' 

                    CoreMessageHandler(message:="lot as no property set attached to it - value cannot be retrieved", messagetype:=otCoreMessageType.ApplicationError, _
                                   arg1:=id, objectname:=Me.ObjectID, subname:="ObjectPropertyValueLot.GetPropertyValue")
                    Return False


                ElseIf _setids.Count = 1 Then
                    ReDim names(1)
                    names(0) = _setids(0)
                    names(1) = id.ToUpper
                Else
                    CoreMessageHandler(message:="property to be added doesnot exist in this set", messagetype:=otCoreMessageType.ApplicationError, _
                                      arg1:=id, objectname:=Me.ObjectID, subname:="ObjectPropertyValueLot.GetPropertyValue")
                    ''' not found not in
                    Return False
                End If

                ''' extend the properties by this set
            ElseIf names.Count > 1 Then
                If _setids.Contains(names(0)) Then
                    ' fine nothing do to
                Else
                    '' check if the setid exists
                    Dim aPropertySet = ObjectPropertySet.Retrieve(id:=names(0), domainid:=DomainID)
                    If aPropertySet Is Nothing Then
                        '' maybe this was not part of the name ?! in another set as unique name ?
                        If _setids.Count > 0 Then
                            '' search it
                            Dim aSet As ObjectPropertySet
                            Dim found As Boolean = False
                            For Each aSetname As String In _setids
                                aSet = ObjectPropertySet.Retrieve(id:=aSetname, domainid:=DomainID)
                                If aSet IsNot Nothing Then
                                    If aSet.PropertyIDs.Contains(id.ToUpper) Then
                                        names(0) = aSetname.ToUpper
                                        names(1) = id.ToUpper
                                        found = True
                                        Exit For
                                    End If
                                End If
                            Next
                            If Not found Then
                                CoreMessageHandler(message:="property does not exist in any set of the lot", messagetype:=otCoreMessageType.ApplicationError, _
                                               arg1:=id, objectname:=Me.ObjectID, subname:="ObjectPropertyValueLot.GetPropertyValue")
                                Return False
                            End If
                        Else
                            CoreMessageHandler(message:="property set '" & names(0) & "' to be added does not exist", messagetype:=otCoreMessageType.ApplicationError, _
                                                arg1:=id, objectname:=Me.ObjectID, subname:="ObjectPropertyValueLot.GetPropertyValue")
                            Return False
                        End If
                    Else
                        If aPropertySet.PropertyIDs.Contains(names(1)) Then
                            ''' add the set
                            If Not Me.AddSet(names(0)) Then
                                CoreMessageHandler(message:="property set '" & names(0) & "' could not be added to the property value lot", messagetype:=otCoreMessageType.ApplicationError, _
                                                   arg1:=id, objectname:=Me.ObjectID, subname:="ObjectPropertyValueLot.GetPropertyValue")
                                Return False
                            End If
                        Else
                            ''' damm the property doesnot exist in this set ?!
                            ''' 

                        End If
                    End If

                End If


            End If

            If Me.GetRelationStatus(ConstRValues) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRValues)

            If _valuesCollection.ContainsKey(key:={names(0), names(1)}) Then
                value = _valuesCollection.Item(key:={names(0), names(1)}).GetValue(ObjectPropertyValue.ConstFNValue)
                Return True
            Else
                value = Nothing
                Return False
            End If

        End Function

        ''' <summary>
        ''' operation to set a PropertyValue
        ''' </summary>
        ''' <param name="id">the property</param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ormObjectOperationMethod(operationname:=ConstOPSetCompoundValue, tag:=ObjectCompoundEntry.ConstCompoundSetter, _
            parameterEntries:={ObjectCompoundEntry.ConstFNEntryName, ObjectCompoundEntry.ConstFNValues})> _
        Public Function SetPropertyValue(id As String, value As Object) As Boolean
            If Not IsAlive(subname:="SetPropertyValue") Then Return Nothing


            ''' the id should be in a canonical form
            ''' 
            Dim names = Shuffle.NameSplitter(id)

            ''' if we have a set then check 
            If names.Count = 1 Then
                If _setids.Count = 0 Then
                    ''' we could look up if this is unqiue
                    ''' 
                    CoreMessageHandler(message:="property to be added doesnot exist in this set", messagetype:=otCoreMessageType.ApplicationError, _
                                     arg1:=id, objectname:=Me.ObjectID, subname:="ObjectPropertyValueLot.SetPropertyValue")
                    Return False
                ElseIf _setids.Count = 1 Then
                    ReDim names(1)
                    names(0) = _setids(0)
                    names(1) = id.ToUpper
                Else
                    CoreMessageHandler(message:="property to be added doesnot exist in this set", messagetype:=otCoreMessageType.ApplicationError, _
                                      arg1:=id, objectname:=Me.ObjectID, subname:="ObjectPropertyValueLot.SetPropertyValue")
                    ''' not found not in
                    Return False
                End If

                ''' extend the properties by this set
            ElseIf names.Count > 1 And Not _setids.Contains(names(0)) Then
                Dim aPropertySet = ObjectPropertySet.Retrieve(id:=names(0), domainid:=DomainID)
                If aPropertySet Is Nothing Then
                        '' maybe this was not part of the name ?! in another set as unique name ?
                        If _setids.Count > 0 Then
                            '' search it
                            Dim found As Boolean = False
                            For Each aSetname As String In _setids
                                Dim aSet As ObjectPropertySet = ObjectPropertySet.Retrieve(id:=aSetname, domainid:=DomainID)
                                If aSet IsNot Nothing Then
                                    If aSet.PropertyIDs.Contains(id.ToUpper) Then
                                        names(0) = aSetname.ToUpper
                                        names(1) = id.ToUpper
                                        found = True
                                        Exit For
                                    End If
                                End If
                            Next
                            If Not found Then
                            CoreMessageHandler(message:="property does not exist in any set of the lot", messagetype:=otCoreMessageType.ApplicationError, _
                                           arg1:=id, objectname:=Me.ObjectID, subname:="ObjectPropertyValueLot.SetPropertyValue")
                                Return False
                            End If
                        Else
                        CoreMessageHandler(message:="property set '" & names(0) & "' to be added does not exist", messagetype:=otCoreMessageType.ApplicationError, _
                                            arg1:=id, objectname:=Me.ObjectID, subname:="ObjectPropertyValueLot.SetPropertyValue")
                            Return False
                    End If

                       
                End If

                    ''' add the set
                    If Not Me.AddSet(names(0)) Then Return False
                End If


                If Me.GetRelationStatus(ConstRValues) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRValues)

                ''' 
                ''' set the value
                If names.Count > 1 AndAlso _valuesCollection.ContainsKey(key:={names(0), names(1)}) Then
                    ''' check if something is now different
                    ''' 
                    Dim aPropertyvalue As ObjectPropertyValue = _valuesCollection.Item(key:={names(0), names(1)})

                    ''' on success
                    If aPropertyvalue.SetValue(ObjectPropertyValue.ConstFNValue, value) Then

                    End If

                    Return True
                Else
                    CoreMessageHandler(message:="property to be added doesnot exist in this set", messagetype:=otCoreMessageType.ApplicationError, _
                                          arg1:=id, objectname:=Me.ObjectID, subname:="ObjectPropertyValueLot.SetPropertyValue")
                    ''' not found not in
                    Return False
                End If

        End Function

        ''' <summary>
        ''' Add a PropertySet to this lot and creates / retrieves all the values with default values
        ''' 
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddSet(id As String, Optional domainid As String = "") As Boolean
            If Not IsAlive(subname:="AddSet") Then Return Nothing
            id = id.ToUpper
            Dim aPropertySet = ObjectPropertySet.Retrieve(id:=id, domainid:=domainid)
            If aPropertySet Is Nothing Then
                CoreMessageHandler(message:="property set to be added doesnot exist", messagetype:=otCoreMessageType.ApplicationError, _
                                    arg1:=id, objectname:=Me.ObjectID, subname:="ObjectPropertyValueLot.AddSet")
                Return False
            End If

            '''
            ''' add the id -> done by event handling
            'If Not _setids.Contains(id) Then
            '    ReDim Preserve _setids(_setids.GetUpperBound(0) + 1)
            '    _setids(_setids.GetUpperBound(0)) = id
            'End If

            '''
            ''' Add All the values
            For Each aProperty In aPropertySet.Properties
                If Not Me.Values.ContainsKey({id, aProperty.ID}) Then
                    Dim aPropertyValue = ObjectPropertyValue.Create(Me.UID, updc:=Me.UPDC, setid:=id, propertyid:=aProperty.ID)
                    If aPropertyValue Is Nothing Then aPropertyValue = ObjectPropertyValue.Retrieve(Me.UID, updc:=Me.UPDC, setid:=id, propertyid:=aProperty.ID)
                    Me.Values.Add(aPropertyValue)
                End If
            Next

            ''' set the vcalid from
            If Not Me.Validfrom.HasValue Then Me.Validfrom = Date.Now

            Return True
        End Function

        ''' <summary>
        ''' retrieve  the configuration from store
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="domainid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(uid As Long, updc As Long, Optional domainid As String = "") As ObjectPropertyValueLot
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.Retrieve(Of ObjectPropertyValueLot)(pkArray:={uid, updc}, domainID:=domainid)
        End Function

        ''' <summary>
        ''' handler for onCreating Event - generates unique primary key values
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub PropertySet_OnCreating(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnCreating
            Dim uid As Long? = e.Record.GetValue(constFNUID)
            Dim updc As Long? = e.Record.GetValue(ConstFNUpdc)
            Dim tag As String
            If Not uid.HasValue OrElse uid = 0 Then
                tag = constFNUID
                uid = Nothing
                updc = 1
            ElseIf Not updc.HasValue OrElse updc = 0 Then
                updc = Nothing
                tag = ConstFNUpdc
            End If
            Dim primarykey As Object() = {uid, updc}
            If uid Is Nothing OrElse updc Is Nothing Then
                If e.DataObject.PrimaryTableStore.CreateUniquePkValue(pkArray:=primarykey, tag:=tag) Then
                    e.Record.SetValue(constFNUID, primarykey(0))
                    e.Record.SetValue(ConstFNUpdc, primarykey(1))
                    e.Result = True
                    e.Proceed = True
                Else
                    CoreMessageHandler(message:="primary keys couldnot be created ?!", subname:="ConfigSet.OnCreate", messagetype:=otCoreMessageType.InternalError)
                End If
            End If

        End Sub
        ''' <summary>
        ''' creates a persistable configuration
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="domainid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(Optional uid As Long = 0, Optional updc As Long = 0, Optional domainid As String = "") As ObjectPropertyValueLot
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.CreateDataObject(Of ObjectPropertyValueLot)(pkArray:={uid, updc}, domainID:=domainid, checkUnique:=True)
        End Function

        ''' <summary>
        ''' Handler for added PropertyValues
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub valuesCollection_OnAdded(sender As Object, e As Database.ormRelationCollection(Of ObjectPropertyValue).EventArgs) Handles _valuesCollection.OnAdded
            '''
            ''' add the id
            ''' 
            Dim aPropertyValue As ObjectPropertyValue = e.Dataobject
            If aPropertyValue Is Nothing Then
                CoreMessageHandler(message:="something different than ObjectPropertyValue added to valuescollection", subname:="_ValuesCollection_OnAdded", _
                                   arg1:=e.Dataobject.ObjectID, objectname:=Me.ObjectID, messagetype:=otCoreMessageType.InternalError)
                Return
            End If
            If Not _setids.Contains(aPropertyValue.SetID) Then
                ReDim Preserve _setids(_setids.GetUpperBound(0) + 1)
                _setids(_setids.GetUpperBound(0)) = aPropertyValue.SetID
            End If
            ' register PropertyChange
            AddHandler aPropertyValue.PropertyChanged, AddressOf Me.ObjectPropertyValueLot_PropertyValueChanged
        End Sub

        ''' <summary>
        ''' Handler for ValueChange of PropertyValue
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ObjectPropertyValueLot_PropertyValueChanged(sender As Object, e As ComponentModel.PropertyChangedEventArgs)
            If e.PropertyName = ObjectPropertyValue.ConstFNValue Then
                ''' 
                ''' 
            End If
        End Sub

        ''' <summary>
        ''' clones an value lot to a new updc
        ''' </summary>
        ''' <param name="pkarray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Clone() As ObjectPropertyValueLot
            If Not IsAlive(subname:="Clone") Then Return Nothing

            Try
                Dim primarykey As Object() = {Me.UID, Nothing} ' new updc
                If MyBase.PrimaryTableStore.CreateUniquePkValue(pkArray:=primarykey, tag:=ConstFNUpdc) Then
                    Return Me.Clone(primarykey)
                End If
                Return Nothing
            Catch ex As Exception
                Call CoreMessageHandler(subname:="ObjectPropertyValueLot.Clone", exception:=ex)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' clones an object
        ''' </summary>
        ''' <param name="pkarray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Clone(pkarray() As Object, Optional runtimeOnly As Boolean? = Nothing) As ObjectPropertyValueLot Implements iormCloneable(Of ObjectPropertyValueLot).Clone


            If Not IsAlive(subname:="Clone") Then Return Nothing

            Try

                '*** now we copy the object
                Dim aNewObject As ObjectPropertyValueLot = MyBase.CloneObject(Of ObjectPropertyValueLot)(pkarray)
                Dim anUid As Long = pkarray(0)
                Dim anUpdc As Long = pkarray(1)
                If Not aNewObject Is Nothing Then
                    ' now clone the Members (Milestones)
                    For Each aPropertyValue In _valuesCollection
                        aNewObject.Values.Add(aPropertyValue.Clone(uid:=anUid, updc:=anUpdc, setid:=aPropertyValue.SetID, propertyid:=aPropertyValue.PropertyID))
                    Next

                    Return aNewObject
                End If

                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(subname:="ObjectPropertyValueLot.Clone", exception:=ex)
                Return Nothing
            End Try
        End Function
    End Class


    ''' <summary>
    ''' class for config properties of entities attached to other business objects
    ''' </summary>
    ''' <remarks>
    ''' 
    ''' Design Principles:
    ''' 
    ''' 1. Values should be never created by Create - go over the Value Lot instead. Values are not added automatically to the Lot.
    ''' 
    ''' 2. Values should be never retrieved alone - go over the lot instead.
    ''' 
    ''' </remarks>
    <ormObject(id:=ObjectPropertyValue.ConstObjectID, version:=1, adddomainbehavior:=False, adddeletefieldbehavior:=True, usecache:=True, _
        modulename:=ConstModuleProperties, Title:="Property Value", description:="values of object properties attached to bussiness object")> _
    Public Class ObjectPropertyValue
        Inherits ormDataObject
        Implements iormCloneable(Of ObjectPropertyValue)

        Public Const ConstObjectID = "PropertyValue"

        ''' <summary>
        ''' Table Definition
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTable(version:=1, usecache:=False)> Public Const constTableID = "TBLOBJPROPERTYVALUES"

        ''' <summary>
        ''' Primary KEys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(ReferenceObjectEntry:=ObjectPropertyValueLot.ConstObjectID & "." & ObjectPropertyValueLot.constFNUID, primaryKeyOrdinal:=1, _
              XID:="PV1", lookupPropertyStrings:={LookupProperty.UseForeignKey & "(" & constFKValues & ")"}, validationPropertyStrings:={ObjectValidationProperty.UseLookup})> _
        Public Const constFNUID = ObjectPropertyValueLot.constFNUID

        <ormObjectEntry(ReferenceObjectEntry:=ObjectPropertyValueLot.ConstObjectID & "." & ObjectPropertyValueLot.ConstFNUpdc, primaryKeyordinal:=2, _
             XID:="PV2", lookupPropertyStrings:={LookupProperty.UseForeignKey & "(" & constFKValues & ")"}, validationPropertyStrings:={ObjectValidationProperty.UseLookup})> _
        Public Const ConstFNUpdc = ObjectPropertyValueLot.ConstFNUpdc

        <ormObjectEntry(ReferenceObjectEntry:=ObjectProperty.ConstObjectID & "." & ObjectProperty.ConstFNSetID, primaryKeyordinal:=3, _
            XID:="PV3")> _
        Public Const ConstFNSetID = ObjectProperty.ConstFNSetID

        <ormObjectEntry(ReferenceObjectEntry:=ObjectProperty.ConstObjectID & "." & ObjectProperty.ConstFNPropertyID, primaryKeyordinal:=4, _
            XID:="PV4")> _
        Public Const ConstFNPropertyID = ObjectProperty.ConstFNPropertyID


        ''' <summary>
        '''  Fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Text, isnullable:=True, _
          XID:="PV10", title:="Value", description:="Value in string representation")> Public Const ConstFNValue = "VALUE"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
          useforeignkey:=otForeignKeyImplementation.None, defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormObjectEntry(defaultvalue:=otDataType.Text, typeid:=otDataType.Long, _
                              title:="Datatype", Description:="OTDB field data type")> Public Const ConstFNDatatype As String = "datatype"

        ''' <summary>
        ''' Foreign Keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaForeignKey(entrynames:={constFNUID, ConstFNUpdc}, _
           foreignkeyreferences:={ObjectPropertyValueLot.ConstObjectID & "." & ObjectPropertyValueLot.constFNUID, _
                                  ObjectPropertyValueLot.ConstObjectID & "." & ObjectPropertyValueLot.ConstFNUpdc}, _
           useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFKValues = "FK_PropertyValue_Lot"


        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>

        <ormEntryMapping(EntryName:=constFNUID)> Private _uid As Long = 0
        <ormEntryMapping(EntryName:=ConstFNUpdc)> Private _updc As Long = 0
        <ormEntryMapping(EntryName:=ConstFNSetID)> Private _SetID As String = ""
        <ormEntryMapping(EntryName:=ConstFNPropertyID)> Private _propertyID As String = ""
        <ormEntryMapping(EntryName:=ConstFNValue)> Private _value As String = ""
        <ormEntryMapping(EntryName:=ConstFNDatatype)> Private _datatype As otDataType
        ''' <summary>
        ''' Relations
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkobject:=GetType(ObjectProperty), cascadeOnDelete:=False, cascadeOnUpdate:=False, _
            fromEntries:={ConstFNSetID, ConstFNPropertyID}, toEntries:={ObjectProperty.ConstFNSetID, ObjectProperty.ConstFNPropertyID})> Public Const ConstRProperty = "ObjectProperty"

        <ormEntryMapping(RelationName:=ConstRProperty, infuseMode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand, _
            keyentries:={ObjectProperty.ConstFNPropertyID})> Private WithEvents _propertyDefinition As ObjectProperty


#Region "Properties"

       

        ''' <summary>
        ''' Gets or sets the datatype of the property.
        ''' </summary>
        ''' <value>The datatype.</value>
        Public ReadOnly Property Datatype() As otDataType
            Get
                Return [Property].Datatype
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the entity.
        ''' </summary>
        ''' <value>The entity.</value>
        Public ReadOnly Property [Property]() As ObjectProperty
            Get
                If Not IsAlive(subname:="[Property]") Then Return Nothing
                InfuseRelation(ConstRProperty)
                Return Me._propertyDefinition
            End Get
        End Property

        ''' <summary>
        ''' returns the UID of the configuration set
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property UID()
            Get
                Return _uid
            End Get
        End Property

        ''' <summary>
        ''' returns the UPDC of the configuration set
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property UPDC()
            Get
                Return _updc
            End Get
        End Property
        '' <summary>
        ''' Gets or sets the Property id.
        ''' </summary>
        ''' <value>The properties.</value>
        Public ReadOnly Property PropertyID() As String
            Get
                Return Me._propertyID
            End Get
        End Property
        '' <summary>
        ''' Gets or sets the section id.
        ''' </summary>
        ''' <value>The properties.</value>
        Public ReadOnly Property SetID() As String
            Get
                Return Me._SetID
            End Get
        End Property
        
        ''' <summary>
        ''' Gets or sets the value in string presenation.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property ValueString() As String
            Get
                Return Me._value
            End Get
            Set(value As String)
                SetValue(ConstFNValue, value)
            End Set
        End Property

#End Region



        ''' <summary>
        ''' retrieve  the configuration set value from store
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="domainid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(uid As Long, updc As Long, setid As String, propertyid As String, Optional domainid As String = "") As ObjectPropertyValue
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.Retrieve(Of ObjectPropertyValue)(pkArray:={uid, updc, setid, propertyid, domainid}, domainID:=domainid)
        End Function


        ''' <summary>
        ''' creates a persistable property value collection value
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="domainid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(uid As Long, updc As Long, setid As String, propertyid As String, Optional domainid As String = "") As ObjectPropertyValue
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.CreateDataObject(Of ObjectPropertyValue)(pkArray:={uid, updc, setid, propertyid, domainid}, domainID:=domainid, checkUnique:=True)
        End Function

        ''' <summary>
        ''' onCreating Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ObjectPropertyValue_OnCreating(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnCreating
            ''' check of the property exist
            ''' 
            Dim setid As String = e.Record.GetValue(ConstFNSetID)
            Dim propertyid As String = e.Record.GetValue(ConstFNPropertyID)

            If setid IsNot Nothing AndAlso propertyid IsNot Nothing Then
                ''' to early to set the link but has to be checked anyway
                _propertyDefinition = ObjectProperty.Retrieve(setid:=setid, ID:=propertyid)
                If _propertyDefinition Is Nothing Then
                    CoreMessageHandler(message:="property doesnot exist", arg1:=setid & "." & propertyid, messagetype:=otCoreMessageType.ApplicationError, objectname:=ConstObjectID, _
                                       subname:="ObjectPropertyValue.OnCreating")
                    e.AbortOperation = True
                Else
                    ''' set this too
                    _datatype = _propertyDefinition.Datatype
                End If
            End If
        End Sub

        ''' <summary>
        ''' clone the object with the new primary key
        ''' </summary>
        ''' <param name="pkarray">primary key array</param>
        ''' <remarks></remarks>
        ''' <returns>the new cloned object or nothing</returns>
        Public Function Clone(uid As Long, updc As Long, setid As String, propertyid As String, Optional domainid As String = "") As ObjectPropertyValue
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return Clone(pkArray:={uid, updc, setid, propertyid, domainid})
        End Function
        ''' <summary>
        ''' clone the object with the new primary key
        ''' </summary>
        ''' <param name="pkarray">primary key array</param>
        ''' <remarks></remarks>
        ''' <returns>the new cloned object or nothing</returns>
        Public Function Clone(pkarray As Object(), Optional runtimeOnly As Boolean? = Nothing) As ObjectPropertyValue Implements iormCloneable(Of ObjectPropertyValue).Clone
            Return MyBase.CloneObject(Of ObjectPropertyValue)(newpkarray:=pkarray)
        End Function
    End Class

End Namespace

