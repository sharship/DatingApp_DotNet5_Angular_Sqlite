import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: Partial<User[]>;
  bsModalRef: BsModalRef;

  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe(users => {
      this.users = users;
    });
  }

  openRolesModal(user: User) {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        user,
        roles: this.getRolesArray(user)
      }
    };

    this.bsModalRef = this.modalService.show(RolesModalComponent, config);

    this.bsModalRef.content.updateSelectedRoles.subscribe(selectedRoles => {

      const rolesToUpdate = {
        roles: [...selectedRoles.filter(role => role.checked === true).map(role => role.name)]
      };

      if (rolesToUpdate) {
        this.adminService.updateUserRoles(user.username, rolesToUpdate.roles).subscribe(() => {
          user.roles = [...rolesToUpdate.roles]
        })
      }
      
    });
  }

  // handleSelectedRoles(selectedRoles, user: User) {
  //   const rolesToUpdate = {
  //     roles: [...selectedRoles.filter(role => role.checked === true).map(role => role.name)]
  //   };

  //   if (rolesToUpdate) {
  //     this.adminService.updateUserRoles(user.username, rolesToUpdate.roles).subscribe(() => {
  //       user.roles = [...rolesToUpdate.roles]
  //     })
  //   }
  // }

  private getRolesArray(user: User) {
    const roles = [];  // roles to send to child component
    const userRoles = user.roles; // user's existing roles
    const availableRoles: any[] = [
      { name: 'Admin', value: 'Admin' },
      { name: 'Moderator', value: 'Moderator' },
      { name: 'Member', value: 'Member' },
    ];

    availableRoles.forEach(role => {
      let isMatch = false;

      for (const userRole of userRoles) {
        if (userRole === role.name) {
          isMatch = true;
          role.checked = true;
          roles.push(role);
          break;
        }
      }

      if (!isMatch) {
        role.checked = false;
        roles.push(role);
      }


    })

    return roles;

  }

}
