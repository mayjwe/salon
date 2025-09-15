using salon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace salon.Services
{
    internal class Validator
    {
        public (bool isValid, List<string> errors) ServiceValidator(Service service)
        {
            List<string> errors = new List<string>();

            if (service.Title.Length < 2 || service.Title.Length > 100)
                errors.Add("Название должно содержать от 2 до 100 символов");

            if (service.Cost <= 0)
                errors.Add("Укажите стоимость");

            if (service.DurationInSeconds <= 0)
                errors.Add("Укажите длительность");

            if (service.Discount < 0 || service.Discount >= 100)
                errors.Add("Скидка должна быть от 0 до 100%");

            return (errors.Count == 0, errors);
        }
    }
}
