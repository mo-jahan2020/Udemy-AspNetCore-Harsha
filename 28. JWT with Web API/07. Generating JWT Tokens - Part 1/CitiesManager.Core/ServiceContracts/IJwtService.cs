using CitiesManager.Core.DTO;
using CitiesManager.Core.Identity;
using System;

namespace CitiesManager.Core.ServiceContracts
{
    //3 - اینترفیس jwtService
 public interface IJwtService
 {
  AuthenticationResponse CreateJwtToken(ApplicationUser user);
 }
}
