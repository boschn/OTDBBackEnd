Option Explicit On

REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** CORE HELPER Classes for On Track Database Backend Library
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2013-08-08
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************''' <summary>

Namespace OnTrack.Database

    ''' <summary>
    ''' Converter Class for ORM Data
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Converter

        ''' <summary>
        ''' Converts String to Array
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function String2Array(input As String) As String()
            String2Array = SplitMultbyChar(text:=input, DelimChar:=ConstDelimiter)
            If Not IsArrayInitialized(String2Array) Then
                Return New String() {}
            Else
                Return String2Array
            End If
        End Function
        ''' <summary>
        ''' Converts Array to String
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Array2String(input() As Object) As String
            Dim i As Integer
            If IsArrayInitialized(input) Then
                Dim aStrValue As String = ""
                For i = LBound(input) To UBound(input)
                    If i = LBound(input) Then
                        aStrValue = ConstDelimiter & UCase(input(i).ToString) & ConstDelimiter
                    Else
                        aStrValue = aStrValue & UCase(input(i)) & ConstDelimiter
                    End If
                Next i
                Return aStrValue
            Else
                Return ""
            End If
        End Function
    End Class
    ''' <summary>
    ''' Reflector Class for reflecting ORM Attributes
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Reflector

        ''' <summary>
        ''' returns ORM Attributes out of a table id
        ''' </summary>
        ''' <param name="tableid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetAttributes(tableid As String) As List(Of Attribute)
            Dim aType As Type = ot.GetDataObjectType(tableid:=tableid)
            If aType Is Nothing Then
                Return GetAttributes(ormType:=aType)
            Else
                Return New List(Of Attribute)
            End If
        End Function

        ''' <summary>
        ''' returns ORM Attributes out of a Type
        ''' </summary>
        ''' <param name="ormType"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetAttributes(ormType As Type) As List(Of System.Attribute)
            Dim aFieldList As System.Reflection.FieldInfo()
            Dim anAttributeList As New List(Of System.Attribute)

            Try
                '***
                '*** collect all the attributes first
                '***
                aFieldList = ormType.GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic Or _
                                                  Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static Or _
                                                  Reflection.BindingFlags.FlattenHierarchy)
                '** look into each  Type (Fields)
                For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList

                    If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                        '** Attributes
                        For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                            '** TABLE
                            If anAttribute.GetType().Equals(GetType(ormSchemaTableAttribute)) Then
                                '* set the tablename
                                DirectCast(anAttribute, ormSchemaTableAttribute).TableName = aFieldInfo.GetValue(Nothing).ToString
                                anAttributeList.Add(anAttribute)
                                '** FIELD COLUMN
                            ElseIf anAttribute.GetType().Equals(GetType(ormSchemaColumnAttribute)) Then
                                '* set the cloumn name
                                DirectCast(anAttribute, ormSchemaColumnAttribute).ColumnName = aFieldInfo.GetValue(Nothing).ToString

                                anAttributeList.Add(anAttribute)
                                '** INDEX
                            ElseIf anAttribute.GetType().Equals(GetType(ormSchemaIndexAttribute)) Then
                                '* set the index name
                                DirectCast(anAttribute, ormSchemaIndexAttribute).IndexName = aFieldInfo.GetValue(Nothing).ToString

                                anAttributeList.Add(anAttribute)
                            End If
                        Next
                    End If
                Next

                Return anAttributeList

            Catch ex As Exception

                Call CoreMessageHandler(subname:="Reflector.GetAttribute", exception:=ex)
                Return anAttributeList

            End Try


        End Function
        ''' <summary>
        ''' returns Table Attribute of a given tableid 
        ''' </summary>
        ''' <param name="tableid"></param>
        ''' <param name="columnName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetTableAttribute(tableid As String) As System.Attribute
            Dim aType As Type = ot.GetDataObjectType(tableid:=tableid)
            If aType Is Nothing Then
                Return GetTableAttribute(ormType:=aType)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' returns ORM Table Attribute out of a Type
        ''' </summary>
        ''' <param name="ormType"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetTableAttribute(ormType As Type) As System.Attribute
            Dim aFieldList As System.Reflection.FieldInfo()

            Try
                '***
                '*** collect all the attributes first
                '***
                aFieldList = ormType.GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic Or _
                                                  Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static Or _
                                                  Reflection.BindingFlags.FlattenHierarchy)
                '** look into each  Type (Fields)
                For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList

                    If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                        '** Attributes
                        For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                            ''' Column
                            If anAttribute.GetType().Equals(GetType(ormSchemaTableAttribute)) Then
                                '* set the tablename
                                DirectCast(anAttribute, ormSchemaTableAttribute).TableName = aFieldInfo.GetValue(Nothing).ToString

                                Return anAttribute
                            End If
                        Next
                    End If
                Next

                Return Nothing

            Catch ex As Exception

                Call CoreMessageHandler(subname:="Reflector.GetTableAttribute", exception:=ex)
                Return Nothing

            End Try


        End Function


        ''' <summary>
        ''' returns ColumnSchema Attribute of a given tableid and columnname
        ''' </summary>
        ''' <param name="tableid"></param>
        ''' <param name="columnName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetColumnAttribute(tableid As String, columnName As String) As System.Attribute
            Dim aType As Type = ot.GetDataObjectType(tableid:=tableid)
            If aType Is Nothing Then
                Return GetColumnAttribute(ormType:=aType, columnName:=columnName)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' returns ORM Attributes out of a Type
        ''' </summary>
        ''' <param name="ormType"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetColumnAttribute(ormType As Type, columnName As String) As System.Attribute
            Dim aFieldList As System.Reflection.FieldInfo()

            Try
                '***
                '*** collect all the attributes first
                '***
                aFieldList = ormType.GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic Or _
                                                  Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static Or _
                                                  Reflection.BindingFlags.FlattenHierarchy)
                '** look into each  Type (Fields)
                For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList

                    If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                        '** Attributes
                        For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                            ''' Column
                            If anAttribute.GetType().Equals(GetType(ormSchemaColumnAttribute)) Then
                                If LCase(aFieldInfo.GetValue(Nothing).ToString) = LCase(columnName) Then
                                    '* set the column name
                                    DirectCast(anAttribute, ormSchemaColumnAttribute).ColumnName = aFieldInfo.GetValue(Nothing).ToString

                                    Return anAttribute
                                End If
                            End If
                        Next
                    End If
                Next

                Return Nothing

            Catch ex As Exception

                Call CoreMessageHandler(subname:="Reflector.GetColumnAttribute", exception:=ex)
                Return Nothing

            End Try


        End Function


        ''' <summary>
        ''' returns ColumnSchema Attribute of a given tableid and columnname
        ''' </summary>
        ''' <param name="tableid"></param>
        ''' <param name="columnName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetIndexAttribute(tableid As String, indexName As String) As System.Attribute
            Dim aType As Type = ot.GetDataObjectType(tableid:=tableid)
            If aType Is Nothing Then
                Return GetIndexAttribute(ormType:=aType, indexName:=indexName)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' returns ORM Attributes out of a Type
        ''' </summary>
        ''' <param name="ormType"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetIndexAttribute(ormType As Type, indexName As String) As System.Attribute
            Dim aFieldList As System.Reflection.FieldInfo()

            Try
                '***
                '*** collect all the attributes first
                '***
                aFieldList = ormType.GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic Or _
                                                  Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static Or _
                                                  Reflection.BindingFlags.FlattenHierarchy)
                '** look into each  Type (Fields)
                For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList

                    If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                        '** Attributes
                        For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                            ''' Index
                            If anAttribute.GetType().Equals(GetType(ormSchemaIndexAttribute)) Then
                                If LCase(aFieldInfo.GetValue(Nothing).ToString) = LCase(indexName) Then
                                    '* set the index name
                                    DirectCast(anAttribute, ormSchemaIndexAttribute).IndexName = aFieldInfo.GetValue(Nothing).ToString

                                    Return anAttribute
                                End If
                            End If
                        Next
                    End If
                Next

                Return Nothing

            Catch ex As Exception

                Call CoreMessageHandler(subname:="Reflector.GetIndexAttribute", exception:=ex)
                Return Nothing

            End Try

        End Function


    End Class

End Namespace
