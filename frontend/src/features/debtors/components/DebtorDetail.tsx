import { Debtor } from '../types';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import { Phone, Mail, MapPin, User, Building } from 'lucide-react';

interface DebtorDetailProps {
  open: boolean;
  onClose: () => void;
  debtor: Debtor | null;
}

export function DebtorDetail({ open, onClose, debtor }: DebtorDetailProps) {
  if (!debtor) return null;

  return (
    <Sheet open={open} onOpenChange={onClose}>
      <SheetContent className="sm:max-w-xl overflow-y-auto">
        <SheetHeader>
          <SheetTitle className="flex items-center gap-2">
            {debtor.debtorType === 'Company' ? (
              <Building className="h-5 w-5" />
            ) : (
              <User className="h-5 w-5" />
            )}
            {debtor.displayName}
          </SheetTitle>
        </SheetHeader>

        <div className="mt-6 space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">External ID</p>
              <p className="font-medium">{debtor.externalId}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Type</p>
              <Badge variant={debtor.debtorType === 'Company' ? 'secondary' : 'outline'}>
                {debtor.debtorType}
              </Badge>
            </div>
            {debtor.taxId && (
              <div>
                <p className="text-sm text-muted-foreground">Tax ID</p>
                <p className="font-medium">{debtor.taxId}</p>
              </div>
            )}
            {debtor.dateOfBirth && (
              <div>
                <p className="text-sm text-muted-foreground">Date of Birth</p>
                <p className="font-medium">{new Date(debtor.dateOfBirth).toLocaleDateString()}</p>
              </div>
            )}
          </div>

          {debtor.notes && (
            <div>
              <p className="text-sm text-muted-foreground">Notes</p>
              <p className="font-medium">{debtor.notes}</p>
            </div>
          )}

          <Card>
            <CardHeader className="py-3">
              <CardTitle className="text-base">Contacts ({debtor.contacts.length})</CardTitle>
            </CardHeader>
            <CardContent className="py-0 pb-3">
              {debtor.contacts.length === 0 ? (
                <p className="text-sm text-muted-foreground">No contacts</p>
              ) : (
                <div className="space-y-2">
                  {debtor.contacts.map((contact) => (
                    <div key={contact.id} className="flex items-center gap-2">
                      {contact.type === 'Phone' ? (
                        <Phone className="h-4 w-4 text-muted-foreground" />
                      ) : (
                        <Mail className="h-4 w-4 text-muted-foreground" />
                      )}
                      <span>{contact.value}</span>
                      {contact.label && (
                        <span className="text-sm text-muted-foreground">({contact.label})</span>
                      )}
                      {contact.isPrimary && <Badge variant="outline">Primary</Badge>}
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="py-3">
              <CardTitle className="text-base">Addresses ({debtor.addresses.length})</CardTitle>
            </CardHeader>
            <CardContent className="py-0 pb-3">
              {debtor.addresses.length === 0 ? (
                <p className="text-sm text-muted-foreground">No addresses</p>
              ) : (
                <div className="space-y-3">
                  {debtor.addresses.map((address) => (
                    <div key={address.id} className="flex items-start gap-2">
                      <MapPin className="h-4 w-4 text-muted-foreground mt-0.5" />
                      <div>
                        <p>{address.addressLine1}</p>
                        {address.addressLine2 && <p>{address.addressLine2}</p>}
                        <p>
                          {[address.city, address.state, address.postalCode]
                            .filter(Boolean)
                            .join(', ')}
                        </p>
                        {address.country && <p>{address.country}</p>}
                        <div className="flex gap-2 mt-1">
                          <Badge variant="outline">{address.label}</Badge>
                          {address.isPrimary && <Badge variant="secondary">Primary</Badge>}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </SheetContent>
    </Sheet>
  );
}
