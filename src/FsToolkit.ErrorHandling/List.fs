namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module List =

    let rec private traverseResultM' (state: Result<_, _>) (f: _ -> Result<_, _>) xs =
        match xs with
        | [] ->
            state
            |> Result.map List.rev
        | x :: xs ->
            let r =
                result {
                    let! y = f x
                    let! ys = state
                    return y :: ys
                }

            match r with
            | Ok _ -> traverseResultM' r f xs
            | Error _ -> r

    let rec private traverseAsyncResultM'
        (state: Async<Result<_, _>>)
        (f: _ -> Async<Result<_, _>>)
        xs
        =
        match xs with
        | [] ->
            state
            |> AsyncResult.map List.rev
        | x :: xs ->
            async {
                let! r =
                    asyncResult {
                        let! ys = state
                        let! y = f x
                        return y :: ys
                    }

                match r with
                | Ok _ -> return! traverseAsyncResultM' (Async.singleton r) f xs
                | Error _ -> return r
            }

    let traverseResultM f xs = traverseResultM' (Ok []) f xs

    let sequenceResultM xs = traverseResultM id xs

    let traverseAsyncResultM f xs =
        traverseAsyncResultM' (AsyncResult.retn []) f xs

    let sequenceAsyncResultM xs = traverseAsyncResultM id xs

    let rec private traverseResultA' state f xs =
        match xs with
        | [] ->
            state
            |> Result.eitherMap List.rev List.rev
        | x :: xs ->

            match state, f x with
            | Ok ys, Ok y -> traverseResultA' (Ok(y :: ys)) f xs
            | Error errs, Error e -> traverseResultA' (Error(e :: errs)) f xs
            | Ok _, Error e -> traverseResultA' (Error [ e ]) f xs
            | Error e, Ok _ -> traverseResultA' (Error e) f xs

    let rec private traverseAsyncResultA' state f xs =
        match xs with
        | [] ->
            state
            |> AsyncResult.eitherMap List.rev List.rev

        | x :: xs ->
            async {
                let! s = state
                let! fR = f x

                match s, fR with
                | Ok ys, Ok y -> return! traverseAsyncResultA' (AsyncResult.retn (y :: ys)) f xs
                | Error errs, Error e ->
                    return! traverseAsyncResultA' (AsyncResult.returnError (e :: errs)) f xs
                | Ok _, Error e ->
                    return! traverseAsyncResultA' (AsyncResult.returnError [ e ]) f xs
                | Error e, Ok _ -> return! traverseAsyncResultA' (AsyncResult.returnError e) f xs
            }

    let traverseResultA f xs = traverseResultA' (Ok []) f xs

    let sequenceResultA xs = traverseResultA id xs

    let rec private traverseValidationA' state f xs =
        match xs with
        | [] ->
            state
            |> Result.eitherMap List.rev List.rev
        | x :: xs ->
            let fR = f x

            match state, fR with
            | Ok ys, Ok y -> traverseValidationA' (Ok(y :: ys)) f xs
            | Error errs1, Error errs2 ->
                traverseValidationA'
                    (Error(
                        errs2
                        @ errs1
                    ))
                    f
                    xs
            | Ok _, Error errs
            | Error errs, Ok _ -> traverseValidationA' (Error errs) f xs

    let traverseValidationA f xs = traverseValidationA' (Ok []) f xs

    let sequenceValidationA xs = traverseValidationA id xs

    let traverseAsyncResultA f xs =
        traverseAsyncResultA' (AsyncResult.retn []) f xs

    let sequenceAsyncResultA xs = traverseAsyncResultA id xs

    let rec private traverseOptionM' (state: Option<_>) (f: _ -> Option<_>) xs =
        match xs with
        | [] ->
            state
            |> Option.map List.rev
        | x :: xs ->
            let r =
                option {
                    let! y = f x
                    let! ys = state
                    return y :: ys
                }

            match r with
            | Some _ -> traverseOptionM' r f xs
            | None -> r

    let rec private traverseAsyncOptionM' (state: Async<Option<_>>) (f: _ -> Async<Option<_>>) xs =
        match xs with
        | [] ->
            state
            |> AsyncOption.map List.rev
        | x :: xs ->
            async {
                let! o =
                    asyncOption {
                        let! y = f x
                        let! ys = state
                        return y :: ys
                    }

                match o with
                | Some _ -> return! traverseAsyncOptionM' (Async.singleton o) f xs
                | None -> return o
            }

    let traverseOptionM f xs = traverseOptionM' (Some []) f xs

    let sequenceOptionM xs = traverseOptionM id xs

    let traverseAsyncOptionM f xs =
        traverseAsyncOptionM' (AsyncOption.retn []) f xs

    let sequenceAsyncOptionM xs = traverseAsyncOptionM id xs

#if !FABLE_COMPILER
    let rec private traverseVOptionM' (state: voption<_>) (f: _ -> voption<_>) xs =
        match xs with
        | [] ->
            state
            |> ValueOption.map List.rev
        | x :: xs ->
            let r =
                voption {
                    let! y = f x
                    let! ys = state
                    return y :: ys
                }

            match r with
            | ValueSome _ -> traverseVOptionM' r f xs
            | ValueNone -> r

    let traverseVOptionM f xs = traverseVOptionM' (ValueSome []) f xs

    let sequenceVOptionM xs = traverseVOptionM id xs

#endif
