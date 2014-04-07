
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

Namespace OnTrack.ObjectProperties
    ''' <summary>
    ''' Enumeration and other definitions
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otLinkType
        One2One = 1
    End Enum

    ''' <summary>
    ''' class to define a set of properties attachable to other business objects
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=PropertySet.ConstObjectID, version:=1, adddomainbehavior:=True, adddeletefieldbehavior:=True, usecache:=True, _
        modulename:=ConstModuleProperties, Title:="Property section", description:="definition of a section of properties attachable to bussiness object")> _
    Public Class PropertySet
        Inherits ormDataObject

        Public Const ConstObjectID = "PropertySet"

        ''' <summary>
        ''' Table Definition
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTable(version:=1, usecache:=True)> Public Const constTableID = "TBLDEFOBJPROPERTYSETS"

        '** primary Keys
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, primaryKeyOrdinal:=1, _
            properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty},
            XID:="OPS1", title:="Set ID", description:="ID of the property set")> Public Const ConstFNSetID = "SETID"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=2 _
         , useforeignkey:=otForeignKeyImplementation.NativeDatabase, defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormObjectEntry(typeid:=otFieldDataType.Text, isnullable:=True, _
          XID:="OPS3", title:="Description", description:="description of the property section")> Public Const ConstFNDescription = "DESC"

        <ormObjectEntry(typeid:=otFieldDataType.List, isnullable:=True, _
          XID:="OPS4", title:="Properties", description:="properties of the object property section")> Public Const ConstFNProperties = "PROPERTIES"

        <ormObjectEntry(typeid:=otFieldDataType.List, isnullable:=True, _
         XID:="OPS5", title:="Business Objects", description:="applicable business objects for this section")> Public Const ConstFNObjects = "OBJECTS"

        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>

        <ormEntryMapping(EntryName:=ConstFNSetID)> Private _id As String = ""
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNProperties)> Private _properties As String()
        <ormEntryMapping(EntryName:=ConstFNObjects)> Private _objects As String()

        ''' <summary>
        ''' Relations
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaRelation(linkobject:=GetType(ObjectProperty), cascadeOnDelete:=True, cascadeOnUpdate:=True, _
            fromEntries:={ConstFNSetID}, toEntries:={ObjectProperty.ConstFNSetID})> Public Const ConstRProperties = "PROPERTIES"

        <ormEntryMapping(RelationName:=ConstRProperties, infuseMode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand, _
            keyentries:={ObjectProperty.ConstFNPropertyID})> Private WithEvents _entitiesCollection As New ormRelationCollection(Of ObjectProperty)(Me, {ObjectProperty.ConstFNPropertyID})

#Region "Properties"

        '' <summary>
        ''' Gets or sets the properties.
        ''' </summary>
        ''' <value>The properties.</value>
        Public Property Objects() As String()
            Get
                Return Me._objects
            End Get
            Set(value As String())
                SetValue(ConstFNObjects, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the properties.
        ''' </summary>
        ''' <value>The properties.</value>
        Public Property Properties() As String()
            Get
                Return Me._properties
            End Get
            Set(value As String())
                SetValue(ConstFNProperties, value)
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
        ''' returns the Entities of this Section
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Entities As ormRelationCollection(Of ObjectProperty)
            Get
                Return _entitiesCollection
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
        Public Overloads Shared Function Retrieve(id As String, Optional domainid As String = "")
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.Retrieve(Of PropertySet)(pkArray:={id.ToUpper, domainid.ToUpper}, domainID:=domainid)
        End Function

        ''' <summary>
        ''' creates a persistable property section
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="domainid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(id As String, Optional domainid As String = "")
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.CreateDataObject(Of PropertySet)(pkArray:={id.ToUpper, domainid.ToUpper}, domainID:=domainid, checkUnique:=True)
        End Function



    End Class

    ''' <summary>
    ''' class to define a configuration entity as member of a configuration attachable to other business objects
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=ObjectProperty.ConstObjectID, version:=1, adddomainbehavior:=True, adddeletefieldbehavior:=True, usecache:=True, _
        modulename:=ConstModuleProperties, Title:="property definition", description:="definition of a property attachable to business objects")> _
    Public Class ObjectProperty
        Inherits ObjectCompoundEntry

        Public Shadows Const ConstObjectID = "OBJECTPROPERTY"

        ''' <summary>
        ''' Table Definition
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTable(version:=1, usecache:=True)> Public Shadows Const ConstTableID = "TBLDEFOBJPROPERTY"

        ''' <summary>
        ''' primary keys
        ''' </summary>
        ''' <remarks></remarks>

        <ormObjectEntry(referenceObjectEntry:=PropertySet.ConstObjectID & "." & PropertySet.ConstFNSetID, primarykeyordinal:=1 _
         , defaultvalue:=ConstGlobalDomain)> Public Const ConstFNSetID = PropertySet.ConstFNSetID

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, primaryKeyOrdinal:=2, _
            properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty},
            XID:="OPR2", title:="Name", description:="ID of the property")> Public Const ConstFNPropertyID = "PROPERTYID"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=3 _
         , useforeignkey:=otForeignKeyImplementation.None, defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormSchemaForeignKey(entrynames:={ConstFNSetID, ConstFNDomainID}, _
            foreignkeyreferences:={PropertySet.ConstObjectID & "." & PropertySet.ConstFNSetID, PropertySet.ConstObjectID & "." & PropertySet.ConstFNDomainID}, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFKEntities = "FK_ObjProperty_Sections"

        ''' <summary>
        ''' other fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otFieldDataType.List, isnullable:=True, _
          XID:="OPR4", title:="Extended Properties", description:="internal properties of the object property")> Public Shadows Const ConstFNExtProperties = "EXTPROPERTIES"

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
        ''' Handles OnCreating and Relation to ConfigSection
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OBjectProperty_OnCreating(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnCreating
            Dim my As ObjectProperty = TryCast(e.DataObject, ObjectProperty)

            If my IsNot Nothing Then
                Dim sectionid As String = e.Record.GetValue(ConstFNSetID)
                If sectionid Is Nothing Then
                    CoreMessageHandler(message:="section doesnot exist", subname:="ConfigEntity.OnCreating", _
                                       messagetype:=otCoreMessageType.ApplicationError, _
                                       arg1:=my.SetID)
                    e.AbortOperation = True
                    Return
                End If
                Dim mySection As PropertySet = PropertySet.Retrieve(id:=sectionid)
                If mySection Is Nothing Then
                    CoreMessageHandler(message:="section doesnot exist", subname:="ConfigEntity.OnCreating", _
                                       messagetype:=otCoreMessageType.ApplicationError, _
                                       arg1:=my.SetID)
                    e.AbortOperation = True
                    Return
                End If
            End If
        End Sub

        ''' <summary>
        ''' Handles OnCreating and Relation to ConfigSection
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub ObjectProperty_OnInfused(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.ClassOnInfused
            Dim my As ObjectProperty = TryCast(e.DataObject, ObjectProperty)

            If my IsNot Nothing Then

                Dim mySet As PropertySet = PropertySet.Retrieve(id:=my.SetID)
                If mySet Is Nothing Then
                    CoreMessageHandler(message:="set doesnot exist", subname:="ConfigEntity.OnCreating", _
                                       messagetype:=otCoreMessageType.ApplicationError, _
                                       arg1:=my.SetID)
                    e.AbortOperation = True
                    Return
                Else
                    mySet.Entities.Add(my)
                End If
            End If
        End Sub
        ''' <summary>
        ''' create a persistable ConfigEntity
        ''' </summary>
        ''' <param name="Section"></param>
        ''' <param name="Entity"></param>
        ''' <param name="domainid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(SetID As String, ID As String, Optional domainid As String = "") As ObjectProperty
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim primarykey As Object() = {SetID.ToUpper, ID.ToUpper, domainid}
            Return ormDataObject.CreateDataObject(Of ObjectProperty)(pkArray:=primarykey, domainID:=domainid, checkUnique:=True)
        End Function

        ''' <summary>
        ''' create a persistable ConfigEntity
        ''' </summary>
        ''' <param name="Section"></param>
        ''' <param name="Entity"></param>
        ''' <param name="domainid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(SetID As String, ID As String, Optional domainid As String = "") As ObjectProperty
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim primarykey As Object() = {SetID.ToUpper, ID.ToUpper, domainid}
            Return ormDataObject.Retrieve(Of ObjectProperty)(pkArray:=primarykey)
        End Function
    End Class

    ''' <summary>
    ''' the Property LINK class links a busines object to a value collection
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=PropertyLink.ConstObjectID, modulename:=ConstModuleProperties, Version:=1, adddomainbehavior:=True, adddeletefieldbehavior:=True, _
        description:="link definitions between properties via value collection and other business objects")> _
    Public Class PropertyLink
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "PropertyLink"

        '** Schema Table
        <ormSchemaTable(version:=1)> Public Const ConstTableID = "TBLOBJPROPERTYLINKS"

        '** index
        <ormSchemaIndex(columnname1:=ConstFNToObjectID, columnname2:=ConstFNToUid, columnname3:=ConstFNFromObjectID, columnname4:=ConstFNFromUid)> Public Const ConstIndTag = "used"

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
            values:={Deliverable.ConstObjectID, Parts.Part.ConstObjectID, Configurables.ConfigCondition.ConstObjectID}, _
            dbdefaultvalue:=Deliverable.ConstObjectID, defaultvalue:=Deliverable.ConstObjectID, _
            XID:="OPL1", title:="From Object", description:="from object id of the business object")> _
        Public Const ConstFNFromObjectID = "FROMOBJECTID"

        <ormObjectEntry(typeid:=otFieldDataType.Long, primarykeyordinal:=2, dbdefaultvalue:="0", lowerrange:=0, _
            XID:="OPL2", title:="Linked from UID", description:="from uid of the business object")> _
        Public Const ConstFNFromUid = "FROMUID"

        <ormObjectEntry(typeid:=otFieldDataType.Long, primarykeyordinal:=3, dbdefaultvalue:="0", lowerrange:=0, _
            XID:="OPL3", title:="Linked from UPDC", description:="from uid of the business object")> _
        Public Const ConstFNFromUpdc = "FROMUPDC"

        ' deactivate ForeignKEy we do not have this object in domains
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=4, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        ''' <summary>
        ''' fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceobjectentry:=ObjectDefinition.ConstObjectID & "." & ObjectDefinition.ConstFNID, _
             properties:={ObjectEntryProperty.Keyword}, _
             validationPropertyStrings:={ObjectValidationProperty.NotEmpty, ObjectValidationProperty.UseLookup}, _
             LookupPropertyStrings:={LookupProperty.UseAttributeValues}, _
             values:={PropertyValueLot.ConstObjectID}, dbdefaultvalue:=PropertyValueLot.ConstObjectID, defaultvalue:=PropertyValueLot.ConstObjectID, _
            XID:="OPL4", title:="Linked to Object", description:="object link to the config object")> _
        Public Const ConstFNToObjectID = "TOOBJECTID"

        <ormObjectEntry(typeid:=otFieldDataType.Long, dbdefaultvalue:="0", lowerrange:=0, _
            XID:="OPL5", title:="Linked to UID", description:="uid link to the config object")> _
        Public Const ConstFNToUid = "TOUID"

        <ormObjectEntry(typeid:=otFieldDataType.Long, isnullable:=True, lowerrange:=0, _
            XID:="OPL6", title:="Linked to UPDC", description:="uid link to the config object")> _
        Public Const ConstFNToUpdc = "TOUPDC"


        '** fields
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, _
            XID:="OPL10", title:="Linke Type", description:="object link type")> Public Const ConstFNTypeID = "typeid"

        '** Mapping
        <ormEntryMapping(EntryName:=ConstFNFromObjectID)> Private _FromObjectID As String
        <ormEntryMapping(EntryName:=ConstFNFromUid)> Private _FromUid As Long
        <ormEntryMapping(EntryName:=ConstFNFromUpdc)> Private _FromUpdc As Long
        <ormEntryMapping(EntryName:=ConstFNToObjectID)> Private _ToObjectID As String
        <ormEntryMapping(EntryName:=ConstFNToUid)> Private _ToUid As Long
        <ormEntryMapping(EntryName:=ConstFNToUpdc)> Private _ToUpdc As Long
        <ormEntryMapping(EntryName:=ConstFNTypeID)> Private _type As otLinkType


#Region "properties"

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
        ''' gets or sets the objectID of the linked object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ToObjectID() As String

            Get
                Return _ToObjectID
            End Get
            Set(value As String)
                SetValue(ConstFNToObjectID, value)
            End Set

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
        ''' Event Handler for on Creating for validating the keys
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub PropertyLink_OnCreating(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnCreating

        End Sub

        ''' <summary>
        ''' Event Handler for validating
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub PropertyLink_OnValidating(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnValidating

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
                                                Optional toUpdc As Long? = Nothing) As PropertyLink
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim primarykey As Object() = {fromObjectID, fromuid, fromupdc, domainid}

            '' set values
            Dim aRecord As New ormRecord
            With aRecord
                .SetValue(ConstFNFromObjectID, fromObjectID)
                .SetValue(ConstFNFromUid, fromuid)
                .SetValue(ConstFNFromUpdc, fromupdc)
                .SetValue(ConstFNDomainID, domainid)
                .SetValue(ConstFNToObjectID, PropertyValueLot.ConstObjectID)
                .SetValue(ConstFNToUid, toUID)
                .SetValue(ConstFNToUpdc, toUpdc)
            End With

            Return ormDataObject.CreateDataObject(Of PropertyLink)(aRecord, checkUnique:=True)
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
        Public Overloads Shared Function Retrieve(fromObjectID As String, fromUid As Long, fromUpdc As Long, Optional domainid As String = "") As PropertyLink
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim primarykey As Object() = {fromObjectID, fromUid, fromUpdc, domainid}
            Return ormDataObject.Retrieve(Of PropertyLink)(primarykey)
        End Function
    End Class

    ''' <summary>
    ''' class for a lot or set of object properties values  attached to other business objects
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=PropertyValueLot.ConstObjectID, version:=1, adddomainbehavior:=False, adddeletefieldbehavior:=True, usecache:=True, _
        modulename:=ConstModuleProperties, Title:="Property Value Lot", description:="Lot of properties values attached to bussiness object")> _
    Public Class PropertyValueLot
        Inherits ormDataObject

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
        <ormObjectEntry(typeid:=otFieldDataType.Long, primaryKeyOrdinal:=1, dbdefaultvalue:="0", _
              XID:="PLOT1", title:="Lot UID", description:="UID of the property value lot")> Public Const constFNUID = "PUID"

        <ormObjectEntry(typeid:=otFieldDataType.Long, dbdefaultvalue:="0", primaryKeyordinal:=2, _
            title:="update count", Description:="Update count of the property value lot", XID:="PLOT2")> Public Const ConstFNUpdc = "UPDC"

        ''' <summary>
        ''' fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, defaultvalue:=ConstGlobalDomain, _
          useforeignkey:=otForeignKeyImplementation.None, dbdefaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormObjectEntry(typeid:=otFieldDataType.Text, isnullable:=True, _
          XID:="PLOT3", title:="Description", description:="description of the property value lot")> Public Const ConstFNDescription = "DESC"

        <ormObjectEntry(typeid:=otFieldDataType.List, _
         lookupPropertyStrings:={LookupProperty.UseObject & "(" & PropertySet.ConstObjectID & ")"}, validationPropertyStrings:={ObjectValidationProperty.UseLookup}, _
         XID:="PLOT4", title:="Property Sets", description:="applicable property sets for this lot")> Public Const ConstFNSets = "SETS"

        <ormObjectEntry(typeid:=otFieldDataType.Date, isnullable:=True, _
        XID:="PLOT11", title:="valid from", description:="property set is valid from ")> Public Const ConstFNValidFrom = "validfrom"

        <ormObjectEntry(typeid:=otFieldDataType.Date, isnullable:=True, _
       XID:="PLOT12", title:="valid until", description:="property set is valid until ")> Public Const ConstFNValiduntil = "validuntil"


        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>

        <ormEntryMapping(EntryName:=constFNUID)> Private _uid As Long = 0
        <ormEntryMapping(EntryName:=ConstFNUpdc)> Private _updc As Long = 0
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNSets)> Private _setids As String()
        <ormEntryMapping(EntryName:=ConstFNValidFrom)> Private _validfrom As DateTime?
        <ormEntryMapping(EntryName:=ConstFNValiduntil)> Private _validuntil As DateTime?

        ''' <summary>
        ''' Relations
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaRelation(linkobject:=GetType(PropertyValue), cascadeOnDelete:=True, cascadeOnUpdate:=True, _
            fromEntries:={constFNUID, ConstFNUpdc}, toEntries:={PropertyValue.constFNUID, PropertyValue.ConstFNUpdc})> Public Const ConstRValues = "PROPERTYVALUES"

        <ormEntryMapping(RelationName:=ConstRValues, infuseMode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand, _
            keyentries:={PropertyValue.ConstFNPropertyID})> Private WithEvents _valuesCollection As New ormRelationCollection(Of PropertyValue)(Me, {PropertyValue.ConstFNPropertyID})

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
        Public Property SectionIDs() As String()
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
        ReadOnly Property Values As ormRelationCollection(Of PropertyValue)
            Get
                Return _valuesCollection
            End Get
        End Property

#End Region

        ''' <summary>
        ''' retrieve  the configuration from store
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="domainid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(uid As Long, updc As Long, Optional domainid As String = "")
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.Retrieve(Of PropertyValueLot)(pkArray:={uid, updc}, domainID:=domainid)
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
            Dim primarykey As Object() = {uid, updc}

            If Not uid.HasValue OrElse uid = 0 Then
                uid = Nothing
                updc = Nothing
            ElseIf Not updc.HasValue OrElse updc = 0 Then
                updc = Nothing
            End If

            If uid Is Nothing OrElse updc Is Nothing Then
                If e.DataObject.PrimaryTableStore.CreateUniquePkValue(pkArray:=primarykey) Then
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
        Public Overloads Shared Function Create(Optional uid As Long = 0, Optional updc As Long = 0, Optional domainid As String = "")
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.CreateDataObject(Of PropertyValueLot)(pkArray:={uid, updc}, domainID:=domainid, checkUnique:=True)
        End Function



    End Class


    ''' <summary>
    ''' class for config properties of entities attached to other business objects
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=PropertyValue.ConstObjectID, version:=1, adddomainbehavior:=False, adddeletefieldbehavior:=True, usecache:=True, _
        modulename:=ConstModuleProperties, Title:="Property Value", description:="values of object properties attached to bussiness object")> _
    Public Class PropertyValue
        Inherits ormDataObject

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
        <ormObjectEntry(ReferenceObjectEntry:=PropertyValueLot.ConstObjectID & "." & PropertyValueLot.constFNUID, primaryKeyOrdinal:=1, _
              XID:="PV1", lookupPropertyStrings:={LookupProperty.UseForeignKey & "(" & constFKValues & ")"}, validationPropertyStrings:={ObjectValidationProperty.UseLookup})> _
        Public Const constFNUID = PropertyValueLot.constFNUID

        <ormObjectEntry(ReferenceObjectEntry:=PropertyValueLot.ConstObjectID & "." & PropertyValueLot.ConstFNUpdc, primaryKeyordinal:=2, _
             XID:="PV2", lookupPropertyStrings:={LookupProperty.UseForeignKey & "(" & constFKValues & ")"}, validationPropertyStrings:={ObjectValidationProperty.UseLookup})> _
        Public Const ConstFNUpdc = PropertyValueLot.ConstFNUpdc

        <ormObjectEntry(ReferenceObjectEntry:=ObjectProperty.ConstObjectID & "." & ObjectProperty.ConstFNSetID, primaryKeyordinal:=3, _
            XID:="PV3", lookupPropertyStrings:={LookupProperty.UseObject & "(" & PropertySet.ConstObjectID & ")"}, validationPropertyStrings:={ObjectValidationProperty.UseLookup})> _
        Public Const ConstFNSectionID = ObjectProperty.ConstFNSetID

        <ormObjectEntry(ReferenceObjectEntry:=ObjectProperty.ConstObjectID & "." & ObjectProperty.ConstFNPropertyID, primaryKeyordinal:=4, _
            XID:="PV4", lookupPropertyStrings:={LookupProperty.UseObject & "(" & ObjectProperty.ConstObjectID & ")"}, validationPropertyStrings:={ObjectValidationProperty.UseLookup})> _
        Public Const ConstFNPropertyID = ObjectProperty.ConstFNPropertyID



        ''' <summary>
        '''  Fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otFieldDataType.Text, isnullable:=True, _
          XID:="PV10", title:="Value", description:="Value in string representation")> Public Const ConstFNValue = "VALUE"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
          useforeignkey:=otForeignKeyImplementation.None, defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID
        ''' <summary>
        ''' Foreign Keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaForeignKey(entrynames:={constFNUID, ConstFNUpdc}, _
           foreignkeyreferences:={PropertyValueLot.ConstObjectID & "." & PropertyValueLot.constFNUID, PropertyValueLot.ConstObjectID & "." & PropertyValueLot.ConstFNUpdc}, _
           useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFKValues = "FK_PropertyValue_Lot"


        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>

        <ormEntryMapping(EntryName:=constFNUID)> Private _uid As Long = 0
        <ormEntryMapping(EntryName:=ConstFNUpdc)> Private _updc As Long = 0
        <ormEntryMapping(EntryName:=ConstFNSectionID)> Private _sectionid As String = ""
        <ormEntryMapping(EntryName:=ConstFNSectionID)> Private _entityid As String = ""
        <ormEntryMapping(EntryName:=ConstFNValue)> Private _value As String = ""

        ''' <summary>
        ''' Relations
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaRelation(linkobject:=GetType(ObjectProperty), cascadeOnDelete:=False, cascadeOnUpdate:=False, _
            fromEntries:={ConstFNSectionID, ConstFNPropertyID}, toEntries:={ObjectProperty.ConstFNSetID, ObjectProperty.ConstFNPropertyID})> Public Const ConstREntity = "CONFIGENTITY"

        <ormEntryMapping(RelationName:=ConstREntity, infuseMode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand, _
            keyentries:={ObjectProperty.ConstFNPropertyID})> Private WithEvents _propertyDefinition As ObjectProperty


#Region "Properties"

        ''' <summary>
        ''' Gets or sets the entity.
        ''' </summary>
        ''' <value>The entity.</value>
        Public ReadOnly Property Entity() As ObjectProperty
            Get
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
        ''' Gets or sets the section id.
        ''' </summary>
        ''' <value>The properties.</value>
        Public ReadOnly Property SectionID() As String
            Get
                Return Me._sectionid
            End Get
        End Property
        '' <summary>
        ''' Gets or sets the entity id.
        ''' </summary>
        ''' <value>The properties.</value>
        Public ReadOnly Property EntityID() As String
            Get
                Return Me._sectionid
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
        Public Overloads Shared Function Retrieve(uid As Long, updc As Long, setid As String, propertyid As String, Optional domainid As String = "")
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.Retrieve(Of PropertyValueLot)(pkArray:={uid, updc, setid, propertyid, domainid}, domainID:=domainid)
        End Function


        ''' <summary>
        ''' creates a persistable property value collection value
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="domainid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(uid As Long, updc As Long, setid As String, propertyid As String, Optional domainid As String = "")
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Return ormDataObject.CreateDataObject(Of PropertyValueLot)(pkArray:={uid, updc, setid, propertyid, domainid}, domainID:=domainid, checkUnique:=True)
        End Function

    End Class

End Namespace

