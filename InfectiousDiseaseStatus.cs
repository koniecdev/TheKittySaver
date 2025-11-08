        //todo: I believe this could be exctracted
        const double averageOfDaysInOneYear = 365.2425;
        if (lastTestedAt < currentDate.AddDays(-averageOfDaysInOneYear * CatAge.MaximumAllowedValue))
        {
            return Result.Failure<InfectiousDiseaseStatus>(
                DomainErrors.CatEntity.InfectiousDiseaseStatusValueObject.TestDateTooOld);
        }
