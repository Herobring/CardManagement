﻿namespace CardManagement

open CardDomainReadModels
open CardManagement.Common
open CardDomainCommandModels
open CardDomain
open FsToolkit.ErrorHandling

open Errors

module CardWorkflows =
    open CardActions
    open System

    type AsyncResult<'a, 'error> = Async<Result<'a, 'error>>
    type CardNumberString = string
    type GetCard = CardNumberString -> AsyncResult<CardInfoModel, Error>
    type GetCardDetails = CardNumberString -> AsyncResult<CardDetailsModel, Error>
    type GetUser = UserId -> AsyncResult<UserModel, Error>
    type ActivateCard = ActivateCardCommandModel -> AsyncResult<CardInfoModel, Error>
    type DeactivateCard = DeactivateCardCommandModel -> AsyncResult<CardInfoModel, Error>
    type SetDailyLimit = SetDailyLimitCardCommandModel -> AsyncResult<CardInfoModel, Error>

    let getCard (getCardFromDbAsync: CardNumber -> AsyncResult<Card, Error>) : GetCard =
        fun cardNumber ->
            asyncResult {
                let! cardNumber = CardNumber.create cardNumber
                let! card = getCardFromDbAsync cardNumber
                return card |> toCardInfoModel
            }

    let getCardDetails (getCardDetailsFromDbAsync: CardNumber -> AsyncResult<CardDetails, Error>)
        : GetCardDetails =
        fun cardNumber ->
            asyncResult {
                let! cardNumber = CardNumber.create cardNumber
                let! card = getCardDetailsFromDbAsync cardNumber
                return card |> toCardDetailsModel
            }

    let getUser (getUserFromDbAsync: UserId -> AsyncResult<User, Error>) : GetUser =
        fun userId ->
            asyncResult {
                let! user = getUserFromDbAsync userId
                return user |> toUserModel
            }

    let activateCard
        (getCardFromDbAsync: CardNumber -> AsyncResult<Card, Error>)
        (getCardAccountInfoAsync: CardNumber -> AsyncResult<CardAccountInfo, Error>)
        : ActivateCard =
        fun activateCommand ->
            asyncResult{
                let! validCommand = activateCommand |> validateActivateCardCommand
                let! card = validCommand.CardNumber |> getCardFromDbAsync
                let! cardAccountInfo = getCardAccountInfoAsync validCommand.CardNumber
                let activeCard = activate cardAccountInfo card
                return activeCard |> toCardInfoModel
            }

    let deactivateCard
        (getCardFromDbAsync: CardNumber -> AsyncResult<Card, Error>)
        : DeactivateCard =
        fun deactivateCommand ->
            asyncResult {
                let! validCommand = deactivateCommand |> validateDeactivateCardCommand
                let! card = validCommand.CardNumber |> getCardFromDbAsync
                let deactivatedCard = deactivate card
                return deactivatedCard |> toCardInfoModel
            }

    let setDailyLimit
        (currentDate: DateTimeOffset)
        (getCardFromDbAsync: CardNumber -> AsyncResult<Card, Error>)
        : SetDailyLimit =
            fun setDailyLimitCommand ->
                asyncResult {
                    let! validCommand = setDailyLimitCommand |> validateSetDailyLimitCommand
                    let! card = getCardFromDbAsync validCommand.CardNumber
                    let! updatedCard = setDailyLimit currentDate validCommand.DailyLimit card
                    return updatedCard |> toCardInfoModel
                }