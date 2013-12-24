
REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** CACHE Module (all static functions) for OTDB Business Objects
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
Imports System.IO
Imports System.Diagnostics.Debug

Namespace OnTrack

    Module Cache

        Private _CacheRegistery As New Dictionary(Of String, Dictionary(Of String, Object))
        ' Our Dictionary of caches, we are caching unter a tag another dictionay with the actual cache

        ''' <summary>
        ''' register an ObjectTag for Cache
        ''' </summary>
        ''' <param name="ObjectTag"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RegisterCacheFor(ByVal ObjectTag As String) As Boolean

            If Not _CacheRegistery.ContainsKey(key:=ObjectTag) Then
                Call _CacheRegistery.Add(key:=ObjectTag, value:=New Dictionary(Of String, Object))
                registerCacheFor = True
                Exit Function
            End If

            registerCacheFor = False
        End Function

        ''' <summary>
        ''' unregister an object for the cache
        ''' </summary>
        ''' <param name="objectTag"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function UnregisterCacheFor(ByVal objectTag As String) As Boolean

            If _CacheRegistery.ContainsKey(key:=ObjectTag) Then
                Call _CacheRegistery.Remove(key:=ObjectTag)
                UnregisterCacheFor = True
                Exit Function
            End If

            UnregisterCacheFor = False
        End Function

        ''' <summary>
        ''' Load an Object from the Cache
        ''' </summary>
        ''' <param name="object"></param>
        ''' <param name="key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadFromCache(ByVal objecttag As String, ByRef key As Object) As Object
            Dim aCache As New Dictionary(Of String, Object)
            Dim aKey As String
            Dim i As Integer
            Dim anObject As Object

            ' if not registered
            If Not _CacheRegistery.ContainsKey(key:=ObjectTag) Then
                LoadFromCache = Nothing
                Exit Function
            Else
                If IsArray(key) Then
                    aKey = ""
                    For i = LBound(key) To UBound(key)
                        aKey &= ConstDelimiter & key(i)
                    Next i
                Else
                    aKey = key.ToString
                End If
                ' load from Cache
                aCache = _CacheRegistery.Item(key:=ObjectTag)
                If aCache.ContainsKey(aKey) = True Then
                    anObject = aCache.Item(key:=aKey)

                    LoadFromCache = anObject
                    Exit Function
                End If
            End If


            LoadFromCache = Nothing
        End Function

        '************* overload aRecord with data from the local Application data container
        '*************
        ''' <summary>
        ''' overload aRecord with data from the local Application data container
        ''' </summary>
        ''' <param name="objectTag"></param>
        ''' <param name="key"></param>
        ''' <param name="theOBJECT"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddToCache(ByVal objectTag As String, ByRef key As Object, ByRef theOBJECT As Object, Optional force As Boolean = True) As Boolean
            Dim aCache As Dictionary(Of String, Object)
            Dim aKey As String
            Dim i As Integer

            ' if not registered
            If Not _CacheRegistery.ContainsKey(key:=objectTag) Then
                AddToCache = False
                Exit Function
            Else
                If IsArray(key) Then
                    aKey = ""
                    For i = LBound(key) To UBound(key)
                        aKey &= ConstDelimiter & key(i)
                    Next i
                Else
                    aKey = key.ToString
                End If
                ' add to Cache
                aCache = _CacheRegistery.Item(key:=objectTag)

                '* over write
                If aCache.ContainsKey(key:=aKey) And force Then
                    Call aCache.Remove(key:=aKey)
                    '* no force but exists return
                ElseIf aCache.ContainsKey(key:=aKey) And Not force Then
                    Return False
                End If

                '* add to cache
                Call aCache.Add(key:=aKey, value:=theOBJECT)
                Return True
            End If

            Return False
        End Function


    End Module

End Namespace
